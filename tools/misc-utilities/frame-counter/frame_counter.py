#!/usr/bin/env python3
import argparse
import queue
import sys
import tkinter as tk
from pynput import keyboard


class FrameCounterApp:
    def __init__(self, root: tk.Tk, fps: float, inc: str, dec: str, reset: str, toggle: str, copy_key: str):
        self.root = root
        self.fps = fps
        self.inc = inc
        self.dec = dec
        self.reset = reset
        self.toggle = toggle
        self.copy_key = copy_key

        self.frames = 0
        self.active = False
        self.q: "queue.Queue[tuple[str, object]]" = queue.Queue()

        root.title("Frame Counter")
        root.attributes("-topmost", True)
        root.resizable(False, False)

        self.status_var = tk.StringVar()
        tk.Label(root, textvariable=self.status_var, font=("Segoe UI", 14)).pack(padx=12, pady=(12, 6))

        help_text = (
            f"{self.reset} = reset+start    {self.toggle} = pause/resume\n"
            f"{self.dec} = -1 frame       {self.inc} = +1 frame\n"
            f"{self.copy_key} = copy to clipboard\n"
            f"Esc = quit     FPS = {fps:g}"
        )
        tk.Label(root, text=help_text, font=("Segoe UI", 9), justify="left").pack(padx=12, pady=(0, 12))

        self._update_text()

        # Start global listener in background thread
        self.listener = keyboard.Listener(on_press=self._on_press)
        self.listener.start()

        # Poll key events from listener thread
        self.root.after(30, self._poll)

        root.protocol("WM_DELETE_WINDOW", self.quit)

    def _secs(self) -> float:
        return self.frames / self.fps if self.fps > 0 else 0.0

    def _update_text(self) -> None:
        secs = self._secs()
        state = "COUNTING" if self.active else "paused"
        self.status_var.set(f"{state}\nFrames: {self.frames}\nSeconds: {secs:.4f}")

    def _copy_to_clipboard(self) -> None:
        text = f"{self.frames} frames ({self._secs():.4f})"
        # Must run on Tk/main thread: this function is called from _poll()
        try:
            self.root.clipboard_clear()
            self.root.clipboard_append(text)
            # Ensures clipboard contents persist after app exits on some platforms
            self.root.update_idletasks()
        except Exception as e:
            # Keep it simple: print error but don't crash
            print(f"Clipboard copy failed: {e}", file=sys.stderr)

    def _on_press(self, key):
        # Esc quits
        if key == keyboard.Key.esc:
            self.q.put(("quit", None))
            return False  # stop listener

        ch = getattr(key, "char", None)
        if not ch:
            return

        if ch == self.reset:
            self.q.put(("reset", None))
        elif ch == self.toggle:
            self.q.put(("toggle", None))
        elif ch == self.copy_key:
            self.q.put(("copy", None))
        elif ch == self.inc:
            self.q.put(("delta", +1))
        elif ch == self.dec:
            self.q.put(("delta", -1))

    def _poll(self) -> None:
        try:
            while True:
                kind, val = self.q.get_nowait()

                if kind == "quit":
                    self.quit()
                    return

                elif kind == "reset":
                    self.frames = 0
                    self.active = True

                elif kind == "toggle":
                    self.active = not self.active

                elif kind == "copy":
                    self._copy_to_clipboard()

                elif kind == "delta":
                    if self.active:
                        self.frames += int(val)

                self._update_text()

        except queue.Empty:
            pass

        self.root.after(30, self._poll)

    def quit(self) -> None:
        try:
            if getattr(self.listener, "running", False):
                self.listener.stop()
        except Exception:
            pass
        self.root.destroy()


def main() -> int:
    ap = argparse.ArgumentParser(description="Tiny global frame counter for YouTube frame stepping.")
    ap.add_argument("--fps", type=float, default=60.0, help="Framerate (e.g. 60, 59.94, 30)")
    ap.add_argument("--inc", type=str, default=".", help="Increment key (default: .)")
    ap.add_argument("--dec", type=str, default=",", help="Decrement key (default: ,)")
    ap.add_argument("--reset", type=str, default="[", help="Reset+start key (default: [)")
    ap.add_argument("--toggle", type=str, default="]", help="Pause/resume toggle key (default: ])")
    ap.add_argument("--copy", type=str, default="\\", help=r"Copy key (default: \)")
    args = ap.parse_args()

    if args.fps <= 0:
        print("Error: --fps must be > 0", file=sys.stderr)
        return 2

    root = tk.Tk()
    FrameCounterApp(root, args.fps, args.inc, args.dec, args.reset, args.toggle, args.copy)
    root.mainloop()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
