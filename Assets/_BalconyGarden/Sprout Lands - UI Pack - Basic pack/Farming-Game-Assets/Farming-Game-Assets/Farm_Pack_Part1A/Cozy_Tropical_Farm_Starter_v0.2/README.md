# Cozy Tropical Farm Starter — v0.2

Expanded original AI-generated painterly 2D farm artwork with a small matching UI starter set. Includes the complete v0.1 collection plus three orchard species, six more crops, and eight plot tiles. Raster PNG sprites with a dimensional look, not 3D models or a complete farming game.

## Contents

| Collection | Separate PNGs |
|---|---:|
| 18 crops × 5 stages | 90 |
| Mango, apple and banana × 5 stages | 15 |
| 21 planting packets + 21 harvest icons | 42 |
| Unworked, tilled and watered soil mounds | 3 |
| Four tools and four decorations | 8 |
| Inventory panel, item card, four slots, four buttons, coin and gem | 12 |
| Eight diamond plot tiles | 8 |
| **Total** | **178** |

Also included: 21 regular five-frame growth atlases, original source sheets, JSON dimensions/pivots, an offline interaction demo, an assembled tile field preview, overview contact sheets and expansion generation prompts.

## New species

Fruit-bearing plants: mango, apple and banana. Mango and apple are trees; banana uses a leafy pseudostem. Their stages are sapling/sucker → young → mature → flowering → fruit-ready. The demo returns them to the mature stage after harvest as a simplified repeatable game mechanic; this is not a horticultural simulation.

New crops: bell pepper, chili, cucumber, lettuce, sweet potato and peanut. Five stages each: planted → sprout → young → growing → ready. Sweet potato uses a planting slip. Some roots or tubers are visible as stylized growth/harvest cues.

Original crops: carrot, corn, rice, tomato, potato, onion, cabbage, eggplant, pumpkin, strawberry, pineapple and watermelon. The original 107 exported PNGs are retained unchanged.

## Plot tiles

Eight 512 × 256 tiles: grass, unworked soil, dry tilled soil, watered tilled soil, planted soil, fertilized soil, dirt path and stone path. Exports share an exact 2:1 diamond footprint, completed to the common shape from the generated texture. This makes their geometry fit an isometric grid. The painted textures may show seams; there are no transition, edge/corner, road junction or engine-specific autotile sets.

The six new crops and three orchard species have no baked soil, so they can layer over these tiles. The original 12 crop sets retain baked soil mounds. The old bare soil mounds are a separate collection and do not have the new tile footprint.

## Try the demo

Unzip the whole folder, then open `demo/index.html` in a browser. No server, account or network is needed. Select a species, Plant, Grow four times, Harvest, and select produce to Sell one. Orchard plants can Grow twice more after harvest for another fruit cycle. Change the field surface in the plot-tile selector. Inventory filters and Reset are interactive. Demo state is temporary.

`previews/plot_field.png` is an assembled scene using the actual exported tiles and plants. `previews/expansion_assets.jpg` shows the 71 additions. `previews/all_assets.jpg` shows every export. The original inventory concept image is a generated illustration with baked text, not a component or a screenshot of the functional demo.

## Files and dimensions

- `assets/crops/`, `assets/orchard/`: 512 × 768 stage PNGs, shared ground pivot.
- `assets/items/`, `assets/tools/`, `assets/decorations/`: 512 × 512 icons.
- `assets/soil/`: old 512 × 768 soil mound sprites.
- `assets/tiles/`: exact 512 × 256 diamond tiles.
- `assets/ui/`: starter UI components; dimensions in the manifest.
- `atlases/`: five 512 × 768 frames in each 2560 × 768 strip.
- `manifest.json`: file paths, sizes, stage order, pivots and baked-ground flags.
- `source_sheets/`: source images for provenance and alternate extraction.
- `GENERATION_PROMPTS.json`: expansion prompts and correction prompts, using ChatGPT's built-in image generation.
- `IMPORT_GUIDE.md`: coordinate, layering and atlas instructions.
- `LICENSE_DRAFT.txt`: proposed buyer terms for publisher review.

## Preparation and verification

AI-generated imagery was separated by alpha silhouettes, resized, padded and aligned. Tile geometry was standardized and transparent holes in the intended surface completed from nearby texture pixels. The crop and orchard atlases contain static growth states, not frame-by-frame animation. Shared pivots reduce movement between stages; the art is illustrative and stage silhouettes are not guaranteed pixel-identical.

Verification checks are recorded in `VALIDATION.json`: asset count, alpha, canvas sizes, padding, unchanged v0.1 PNGs, atlas frame matching, tile geometry, resource paths and demo interaction logic. Browser rendering and native imports into Unity, Godot and Flutter remain unverified in this environment.

The UI is a starter subset, not the full planned UI expansion. Panel title plaques and leaf ornaments are baked into fixed raster panels; these are not tested nine-slice skins. No layered PSD/Aseprite sources, vectors, 3D models, animals, buildings, audio, collision data or native-engine prefabs are included.

Before sale, review the visual result at your intended game scale, finalize the publisher identity and usage terms, and clearly disclose AI-generated graphics. Describe only supplied and tested features; avoid claims of exclusive or hand-drawn artwork, seamless autotiling, native-engine packages or a complete game.
