# UPixelator

Thank you for purchasing UPixelator! ❤️  
If you like the asset I would appreciate your review!  

**AssetStore Reviews bug:**  
If you only see this message "Please download this asset to leave a review":  
Click on one of the N star blue rows and the "Write a Review" button will show up.  

## Contact
If you have any questions or feedback, please contact me at reslav.hollos@gmail.com.  
You can also join the [Discord server](https://discord.gg/uFEDDpS8ad)!  

## How to update!
### v2 -> v2.1.0
- Please first delete `Assets/Abiogenesis3d` and `Assets/Editor/Abiogenesis3d` folders
- [Pixel Art Edge Highlights] install minimum v1.3 version
### v1 -> v2
- Please first delete `Assets/Abiogenesis3d` and `Assets/Editor/Abiogenesis3d` folders
- [Pixel Art Edge Highlights] install minimum v1.1 version

## Quick start
- Drag and drop `Prefabs/UPixelator` into scene.  
- Or open the scene under the `Example/Scenes` folder.  
- Otherwise go to the [Setup](#setup) section of this readme.  

## Description
`UPixelator` is a shaderless solution for pixelating 3d scenes with pixel creep reduction for orthographic camera.  
It provides the base for creating Pixel Art style games with 3d models.  

[WebGL Demo](https://radivarig.github.io/UPixelator_URP_WebGL)  
[Asset Store](https://assetstore.unity.com/packages/slug/243562)  
[Discord Server](https://discord.gg/gUEgnTkPF2)  

## Modules
- [Pixel Art Edge Highlights](https://assetstore.unity.com/packages/slug/263418)

## Render pipelines
- Built-in ✓
- URP ✓

## Tested builds
Unity 2021.3 (Builtin, URP 12): Windows, WebGL  
Unity 2022.3 (Builtin, URP 14): Windows, WebGL  

## Shaderless
Requires no special shaders so you can keep your existing materials.

## Pixelization
Achieved by rendering to a lower resolution render texture and upscaling to fit the screen.

## Pixel Creep reduction
Camera and tagged objects are snapped to a grid of world space pixel size resulting in the same pixel colors being rendered while moving.

## Subpixel stabilization
Snapping to pixel size grid makes the camera shake so subpixel offset is applied in the game resolution based on the snap position difference.

## UI
Includes scripts for making canvas elements follow a world transform in canvas overlay mode.

## How the asset works
1. Camera gets snapped to a grid of size of a pixel in world space which makes pixel colors consistent but shows a zig-zag movement.  
1. Zig-zag is smoothed out with offseting the upscaled image for the difference from the original camera position.  
1. This makes the scene stable for non moving objects so for moving objects a Snappable script is provided.  

## Camera projections
This asset is intended to be used with orthographic camera, even though it will pixelize a perspective camera.  
Please note that only the orthographic camera has the benefit of pixel creep reduction.  

## Performance
Performance difference is rendering to screen vs. rendering to a texture and a second camera rendering that texture.  
If your original render is heavy you might even get a performance gain since only a part (25% or less) of the pixels are rendered.  

Finding Snappables is cached and not executed every frame.  

## Mouse Events
When rendering to a texture or using multiple cameras, mouse events stop working properly.
This is solved in `MultiCameraEvents` script which is automatically added by the `UPixelator` script.
It works by stopping incorrect default events with `camera.eventMask = 0` and emitting correct ones.

## Please note
- Rotation will always have some pixel creep but it's less noticeable with higher rotation speed
- Zig-zag will occur for all snapped moving objects, but is less noticeable with higher movement speed
- Large screen space effects are not supported but repeating patterns like 2,4,8,16 pixels wide are
- Resolution must be set and be divisible with pixelMultiplier (work in progress on this)

# 
# Setup
- Drag the `UPixelator` prefab into scene and you should immediatelly see the pixelated effect
- Drag the `UPixelator - Canvas` prefab into scene to get the runtime UI controls
- Set a resolution (ex. 1920x1080) that is a multiple of a chosen `UPixelator.pixelMultiplier` number
- Set `Scale: 1` in game window to have pixels render 1 on 1
- [Built-in] If you have postprocess move `PostProcessLayer` and `PostProcessVolume` from MainCamera to the UPixelator gameObject

## Fine tuning
- When setting camera.targetTexture with lower resolution Unity uses mipmaps and reduces the texture resolution
  - [URP] A quick workaround for mipmaps is to set this in the renderer asset `Quality > Upscaling Filter > FidelityFX Super Resolution 1.0`
  - [Built-in] Set texture's MipMaps to Off and texture's filtering to Point
- Lower the texture's resolution
- Increase the shadow resolution
- [Built-in] Set `Quality > Shadow Projection: Stable Fit`

## UI
To make a `RectTranform` follow a world `Transform` parent one under the other and attach `FollowTransformUI.cs`  

## Legacy text font resolution:
  - Set font rendering mode to `Hinted Raster` and character to `ASCII default set`
  - Click `Tripple dot icon (upper right) > Create Editable Copy`
  - Set copied font's texture filtering to Point and lower the resolution
  - Use `GlobalSetFont.cs` with the copied font to update all Text components in the scene

## Demo scene
- Located in `Example/Scenes` folder
- Assets used:
  - If you're interested in these assets it's better to download them directly from the package manager
  - `Unity Technologies > 3d Game Kit` character with modified scripts
  - `Unity Technologies > AR Face Assets` texture and modified mesh
  - Texture's resolution from these assets have been lowered to achieve a smalled unitypackage size

## Known issues
- [URP] Consecutive cameras do not work with `Post Process` enabled [Forum](https://forum.unity.com/threads/1265873/)
- Using MovePosition in FixedUpdate has to be limited to 60fps [Forum](https://forum.unity.com/threads/1389540/)
- Ocassional pixel flicker on alpha clipped textures or geometry edges

## In progress/research
- [WIP] Parallax effect with multiple cameras
- [HDRP] targetTexture with lower resolution does not render full screen rect
- Skinned mesh hierarchy snapping after animation