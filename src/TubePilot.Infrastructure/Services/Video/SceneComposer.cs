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
    public class SceneComposer : ISceneComposer
    {
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
            var fontPath = await _fontProvider.GetFontPathAsync(language, cancellationToken);
            var collection = new FontCollection();
            var family = collection.Add(fontPath);

            // These are single-instance variable fonts (Google Fonts moved almost everything
            // to variable-only distribution) — they typically expose one style, not a
            // separate Bold sub-family, so requesting FontStyle.Bold explicitly can throw.
            // Fall back to the family's default style if Bold isn't available.
            Font MakeFont(float size)
            {
                try { return family.CreateFont(size, FontStyle.Bold); }
                catch (FontFamilyNotFoundException) { return family.CreateFont(size); }
                catch (ArgumentException) { return family.CreateFont(size); }
            }

            using var image = await Image.LoadAsync<Rgba32>(sourceImagePath, cancellationToken);

            image.Mutate(ctx =>
            {
                // Cover-fit into the target frame size (crop to fill, like CSS object-fit: cover).
                ctx.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Crop,
                    Size = new Size(width, height)
                });

                // Dark gradient-ish band at the bottom so white text stays readable over any image.
                var bandHeight = height / 4;
                var band = new RectangularPolygon(0, height - bandHeight, width, bandHeight);
                ctx.Fill(Color.FromRgba(0, 0, 0, 160), band);

                var rankFont = MakeFont(height * 0.10f);
                var titleFont = MakeFont(height * 0.055f);
                var badgeFont = MakeFont(height * 0.035f);

                var padding = width * 0.04f;
                var rankText = $"#{overlay.Rank}";
                ctx.DrawText(rankText, rankFont, Color.White, new PointF(padding, height - bandHeight + (bandHeight * 0.08f)));

                var titlePos = new PointF(padding, height - bandHeight + (bandHeight * 0.45f));
                ctx.DrawText(overlay.Title, titleFont, Color.White, titlePos);

                if (!string.IsNullOrWhiteSpace(overlay.Badge))
                {
                    var badgePos = new PointF(padding, height - (bandHeight * 0.12f));
                    ctx.DrawText(overlay.Badge, badgeFont, Color.FromRgba(255, 210, 90, 255), badgePos);
                }
            });

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath)!);
            await image.SaveAsJpegAsync(outputPath, cancellationToken);
            return outputPath;
        }
    }
}
