# Comfy Cloud workflow templates

The two `.json` files in this folder are **placeholders**, not real workflows. They exist only so
the app has something to load without crashing before you've built the real workflows.

## How to replace them

1. Open the Comfy Cloud web editor (platform.comfy.org / cloud.comfy.org) with an active Standard+ subscription.
2. Start from a template:
   - **text-to-image.json**: a Flux (or SDXL) text-to-image template.
   - **image-to-video.json**: the "Wan 2.2 Image-to-Video" template.
3. Adjust settings as desired (resolution, frame count/fps, steps).
4. Export each via the frontend's **"Export Workflow (API)"** option (not the plain "Save" — the API
   format uses node IDs as top-level keys with `class_type`/`inputs`, which is what `ComfyCloudClient`
   and `WorkflowTemplateLoader` expect).
5. Save the exported JSON over the corresponding placeholder file in this folder.
6. Find the node ID + input name for:
   - the positive prompt text box in `text-to-image.json`
   - the source image (`LoadImage`) node in `image-to-video.json`
   - the positive prompt / motion description box in `image-to-video.json`
7. Update `appsettings.json`'s `ComfyCloud` section (`TextToImagePromptNode`,
   `ImageToVideoImageNode`, `ImageToVideoPromptNode`) to match those node IDs — no code changes needed.
