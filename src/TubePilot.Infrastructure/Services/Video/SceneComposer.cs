using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TubePilot.Application.Interfaces;

namespace TubePilot.Infrastructure.Services.Video
{
    // Burns rank/title/badge text onto a copy of the row's image with real, crisp glyphs —
    // no AI model is ever asked to draw text. Also normalizes every scene to one fixed
    // size/aspect so FFmpeg can concatenate them without per-frame scaling surprises.
    //
    // Styling matches a "timeline countdown" reference look: a blue header banner holding a
    // large yellow rank number with a black stroke (Anton), a white subtitle strip below it
    // holding the title (Bebas Neue), a thin black frame border, and a soft drop shadow under
    // the header — applied to a single full-frame photo/clip per scene, not a multi-photo grid.
    public class SceneComposer : ISceneComposer
    {
        private static readonly Color BannerBlue = Color.FromRgba(23, 132, 186, 255);
        private static readonly Color RankYellow = Color.FromRgba(255, 214, 10, 255);
        private static readonly Color StrokeBlack = Color.FromRgba(15, 15, 15, 255);
        private static readonly Color SubtitleWhite = Color.White;
        private static readonly Color BadgeGold = Color.FromRgba(255, 196, 0, 255);

        private readonly IFontProvider _fontProvider;

        public SceneComposer(IFontProvider fontProvider)
        {
            _fontProvider = fontProvider;
        }

        public async Task<string> ComposeSceneAsync(
            string sourceImagePath,
            SceneTextOverlay overlay,
            string language,
            string outputPath,
            int width,
            int height,
            CancellationToken cancellationToken = default)
        {
            using var image = await Image.LoadAsync<Rgba32>(sourceImagePath, cancellationToken);

            image.Mutate(ctx =>
            {
                // Cover-fit into the target frame size (crop to fill, like CSS object-fit: cover).
                ctx.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Crop,
                    Size = new Size(width, height)
                });
            });

            await DrawBandAndTextAsync(image, overlay, language, width, height, cancellationToken);

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath)!);
            await image.SaveAsJpegAsync(outputPath, cancellationToken);
            return outputPath;
        }

        public async Task<string> ComposeTextOverlayAsync(
            SceneTextOverlay overlay,
            string language,
            string outputPath,
            int width,
            int height,
            CancellationToken cancellationToken = default)
        {
            // Fully transparent canvas — only the band + text get painted on top, so this
            // can be alpha-composited over a real video frame with FFmpeg's `overlay` filter.
            using var image = new Image<Rgba32>(width, height);

            await DrawBandAndTextAsync(image, overlay, language, width, height, cancellationToken);

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath)!);
            await image.SaveAsPngAsync(outputPath, cancellationToken);
            return outputPath;
        }

        private async Task DrawBandAndTextAsync(
            Image<Rgba32> image,
            SceneTextOverlay overlay,
            string language,
            int width,
            int height,
            CancellationToken cancellationToken)
        {
            var bodyFontPath = await _fontProvider.GetFontPathAsync(language, cancellationToken);
            var rankFontPath = await _fontProvider.GetDecorativeFontPathAsync(DecorativeFontRole.RankNumber, cancellationToken);
            var subtitleFontPath = await _fontProvider.GetDecorativeFontPathAsync(DecorativeFontRole.Subtitle, cancellationToken);

            var bodyFamily = new FontCollection().Add(bodyFontPath);
            var rankFamily = new FontCollection().Add(rankFontPath);
            var subtitleFamily = new FontCollection().Add(subtitleFontPath);

            // These are single-instance variable fonts (Google Fonts moved almost everything
            // to variable-only distribution) — they typically expose one style, not a
            // separate Bold sub-family, so requesting FontStyle.Bold explicitly can throw.
            // Fall back to the family's default style if Bold isn't available.
            Font MakeFont(FontFamily family, float size)
            {
                try { return family.CreateFont(size, FontStyle.Bold); }
                catch (FontFamilyNotFoundException) { return family.CreateFont(size); }
                catch (ArgumentException) { return family.CreateFont(size); }
            }

            image.Mutate(ctx =>
            {
                var headerHeight = height * 0.22f;
                var bannerHeight = headerHeight * 0.66f;
                var subtitleHeight = headerHeight - bannerHeight;

                // Header: blue rank banner over a white subtitle strip, framing the top of
                // the frame — the reference's signature look, applied to a single scene image
                // instead of a multi-photo grid.
                ctx.Fill(BannerBlue, new RectangularPolygon(0, 0, width, bannerHeight));
                ctx.Fill(SubtitleWhite, new RectangularPolygon(0, bannerHeight, width, subtitleHeight));

                // Soft drop shadow directly under the header so it reads as raised above the photo.
                var shadowHeight = Math.Max(4, height * 0.012f);
                ctx.Fill(Color.FromRgba(0, 0, 0, 90), new RectangularPolygon(0, headerHeight, width, shadowHeight));

                var rankFont = MakeFont(rankFamily, bannerHeight * 0.62f);
                var subtitleFont = MakeFont(subtitleFamily, subtitleHeight * 0.5f);
                var badgeFont = MakeFont(bodyFamily, height * 0.032f);

                // Big yellow rank number with a black stroke (Anton) — centered in the blue banner.
                var rankText = $"#{overlay.Rank}";
                var rankOptions = new RichTextOptions(rankFont)
                {
                    Origin = new PointF(width / 2f, bannerHeight / 2f),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                ctx.DrawText(rankOptions, rankText, Brushes.Solid(RankYellow), Pens.Solid(StrokeBlack, Math.Max(1.5f, bannerHeight * 0.035f)));

                // Title as the white-strip subtitle (Bebas Neue) — the reference's "age" line role.
                var subtitleOptions = new RichTextOptions(subtitleFont)
                {
                    Origin = new PointF(width / 2f, bannerHeight + subtitleHeight / 2f),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                ctx.DrawText(subtitleOptions, overlay.Title.ToUpperInvariant(), Brushes.Solid(StrokeBlack));

                if (!string.IsNullOrWhiteSpace(overlay.Badge))
                {
                    // Small accent chip in the bottom-right corner, out of the header's way.
                    var padding = width * 0.03f;
                    var badgeOptions = new RichTextOptions(badgeFont)
                    {
                        Origin = new PointF(width - padding, height - padding),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom
                    };
                    var badgeBounds = TextMeasurer.MeasureBounds(overlay.Badge, badgeOptions);
                    var chipPadding = badgeFont.Size * 0.35f;
                    var chip = new RectangularPolygon(
                        badgeBounds.X - chipPadding, badgeBounds.Y - chipPadding,
                        badgeBounds.Width + chipPadding * 2, badgeBounds.Height + chipPadding * 2);
                    ctx.Fill(Color.FromRgba(0, 0, 0, 150), chip);
                    ctx.DrawText(badgeOptions, overlay.Badge, Brushes.Solid(BadgeGold));
                }

                // Thin black frame border around the whole scene.
                var borderThickness = Math.Max(2f, width * 0.004f);
                ctx.Draw(StrokeBlack, borderThickness, new RectangularPolygon(
                    borderThickness / 2, borderThickness / 2,
                    width - borderThickness, height - borderThickness));
            });
        }
    }
}
