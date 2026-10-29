# Import and integration guide

## Coordinates

`manifest.json` describes all 178 assets. Each record includes `file`, `width`, `height`, `visible_bounds`, `pivot_top_left`, and `pivot_bottom_left`. Bounds are pixel rectangles `[left, top, right, bottom]` with right/bottom exclusive. Pivots are normalized coordinates relative to the full transparent canvas, not the visible bounds.

| Asset | Canvas | Pivot, origin at top left | Pivot, origin at bottom left |
|---|---|---|---|
| Crop stages and soil | 512 × 768 | (0.5, 0.9375) | (0.5, 0.0625) |
| Inventory items, tools, decorations | 512 × 512 | (0.5, 0.5) | (0.5, 0.5) |
| Slot and currency UI | 256 × 256 | (0.5, 0.5) | (0.5, 0.5) |
| Button states | 512 × 192 | (0.5, 0.5) | (0.5, 0.5) |
| Inventory panel | 1024 × 576 | (0.5, 0.5) | (0.5, 0.5) |
| Item card | 512 × 576 | (0.5, 0.5) | (0.5, 0.5) |

For ground sprites, draw the top left at `(groundX − 256 × scale, groundY − 720 × scale)` and draw the entire 512 × 768 canvas at the same scale for every stage. Transparent padding is intentional. Do not auto-trim and re-center every frame. The pivot refers to the bottom center of the soil silhouette, not its geometric center or a character's feet.

## Growth atlases

Each atlas has exactly five equal frames. Frame `i` (zero-based) is `[i × 512, 0, 512, 768]`, with no spacing or extruded borders. Use nearest whole-frame coordinates. All five frames share the same pivot. These frames represent gameplay stages; change them on your own growth timer. If your renderer samples adjacent frames at atlas edges, add extrusion in your engine's texture packer before enabling mipmaps; no extrusion is supplied here.

## Transparency and scale

PNG exports use ordinary RGBA alpha. Preserve the alpha channel and use transparent rendering. Smooth filtering generally suits this painterly style. Test both light and dark backgrounds at the final game size. PNG pixel dimensions are export dimensions, not a promise of the original generation resolution. Some components were upsampled during standardization. Keep resource budgets appropriate for your device; smaller copies can be produced from these exports.

## UI components

Use filenames to choose states; the artwork does not implement interactions. Add labels, prices, quantities and dynamic item images in your application. Keep panel aspect ratios when possible. Do not claim these baked-panel designs support arbitrary nine-slice resizing: title plaques and ornaments need separate treatment for a future scalable UI skin. The functional demo deliberately uses CSS layout around the supplied slot and button PNGs; it does not reproduce the generated inventory art concept pixel for pixel.

## Engine-neutral checklist

1. Import a single crop's five stage PNGs without automatic frame trimming.
2. Set the pivot using the coordinate-origin convention your engine expects.
3. Keep scale unchanged while switching stages 1–5.
4. Test stages over light/dark backgrounds and adjacent plots, especially tall crops.
5. Import the matching planting and harvest icons into item slots.
6. Connect actual game data to button states and quantity labels.
7. Repeat for the remaining crops, then profile texture memory on the intended devices.

The demo's resource paths and interaction logic were checked locally. Browser rendering could not be verified in this environment. It is not evidence of a native Unity/Godot/Flutter import. Supply a native sample project before advertising an engine-specific package.


## v0.2 orchard and crop expansion

The six added crops and three orchard plants have `ground_baked: false`. Their ground-contact pivot is (256, 720) on the 512 × 768 canvas. Original crop PNGs retain their soil mounds and have `ground_baked: true`. Do not add plot tiles behind the old crops unless you intend to show both ground styles.

New crop names: bell_pepper, chili, cucumber, lettuce, sweet_potato, peanut. Orchard names: mango, apple, banana. Orchard stages are sapling/sucker, young, mature, flowering, fruit_ready. The demo returns orchard plants to stage 3 after harvest as a simplified regrowth mechanic. These are static illustrative stages; root/tuber visibility and regrowth are game art conventions rather than horticultural instructions.

## Exact diamond plot tiles

Every plot tile is 512 × 256. Its normalized pivot is (0.5, 0.5), its center at (256, 128). With a tile-center origin (originX, originY), row r and column c:

- tile center X = originX + (c − r) × 256 × scale
- tile center Y = originY + (c + r) × 128 × scale
- tile image top left = (centerX − 256 × scale, centerY − 128 × scale)
- new plant image top left = (centerX − 256 × scale, centerY − 720 × scale)

Use the same scale for tile and plant canvases. Render all ground tiles first. Then render plants by increasing row + column so foreground silhouettes overlap the plants behind them. Leave room above the field for tall trees. Shared shape geometry does not guarantee seamless illustrated texture: this eight-tile set has no transition or edge/corner autotiles. The template applies an exact hard diamond alpha mask; filtering and mipmaps may still produce thin sampling seams in some engines.

The tile sheet was normalized from generated diamonds to a shared 2:1 footprint, and the intended surface was completed from nearby texture pixels. `geometry_standardized: true` identifies these exports. For strict renderer-specific seam handling, generate extrusion in your engine's texture packer and verify the result at runtime.
