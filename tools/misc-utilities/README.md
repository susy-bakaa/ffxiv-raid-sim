# misc-tools

This folder contains bunch of smaller tools and utilities that I occasionally use with development of the simulator.

---

## Tools

- frame-counter let's you manually calculate frames and their duration from a video with hotkeys.
- msdfgen let's you generate signed distance field textures used as masks for few shaders in the project.

---

## Output

These tools can generate the following:

- frame-counter: nothing, it is purely for information.
- msdfgen: signed distance field textures.

---

## Requirements

- 64-bit Windows, Linux support currently untested
- Python and the pynput package for frame-counter

---

## How to Use

This section contains simple instructions on how to use these tools. If you want more in-depth instructions check out the raidsim docs.

### frame-counter

- Simply have Python and pynput installed
- Can be used inside a virtual environment
- To use the file, just open your terminal and type "py frame_counter.py --fps 60" or equivalent
- You can use the "--fps" flag to supply the video frame rate you are dissecting

### msdfgen

- Simply run the pre-built executable.
- To use it, just open your terminal and type "msdfgen.exe sdf -svg "path\to\input_image.svg" -o "path\to\output_image.tif" -dimensions 4096 4096 -pxrange 512 -scale 2 -format tiff" or equivalent
- You can use the "-dimensions", "-pxrange" and "-scale" to easily control the output image.
- Input format must be a SVG but output can be a PNG or TIFF for higher quality.
- For more information go look at the msdfgen repository.