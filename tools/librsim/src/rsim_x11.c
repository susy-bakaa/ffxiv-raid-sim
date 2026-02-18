// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
// ---------------------------------------------------------------------
// rsim_x11.c
// X11-only window title setter for Unity Linux player.
// Works on X11 and typically on Wayland via XWayland.
// Build into: librsim.so

#include <X11/Xlib.h>
#include <X11/Xatom.h>
#include <unistd.h>
#include <string.h>
#include <stdlib.h>

static Window g_cachedWindow = 0;

static int get_window_pid(Display* dpy, Window w, Atom atomNetWmPid, pid_t* outPid)
{
    Atom actualType;
    int actualFormat;
    unsigned long nitems, bytesAfter;
    unsigned char* prop = NULL;

    int status = XGetWindowProperty(
        dpy, w, atomNetWmPid,
        0, 1, False, XA_CARDINAL,
        &actualType, &actualFormat, &nitems, &bytesAfter, &prop
    );

    if (status != Success || !prop || nitems < 1) {
        if (prop) XFree(prop);
        return 0;
    }

    unsigned long pidVal = *(unsigned long*)prop;
    XFree(prop);

    *outPid = (pid_t)pidVal;
    return 1;
}

static Window find_window_by_pid(Display* dpy, Window root, pid_t pid, Atom atomNetWmPid)
{
    Window rootRet, parentRet;
    Window* children = NULL;
    unsigned int nchildren = 0;

    if (!XQueryTree(dpy, root, &rootRet, &parentRet, &children, &nchildren))
        return 0;

    Window found = 0;

    for (unsigned int i = 0; i < nchildren && !found; i++) {
        pid_t wpid = -1;
        if (get_window_pid(dpy, children[i], atomNetWmPid, &wpid) && wpid == pid) {
            found = children[i];
            break;
        }
        found = find_window_by_pid(dpy, children[i], pid, atomNetWmPid);
    }

    if (children) XFree(children);
    return found;
}

static void set_title(Display* dpy, Window w, const char* title)
{
    // Legacy: WM_NAME
    XStoreName(dpy, w, title);

    // EWMH: _NET_WM_NAME (UTF8_STRING)
    Atom atomNetWmName = XInternAtom(dpy, "_NET_WM_NAME", False);
    Atom atomUtf8 = XInternAtom(dpy, "UTF8_STRING", False);

    XChangeProperty(
        dpy, w, atomNetWmName,
        atomUtf8, 8, PropModeReplace,
        (const unsigned char*)title, (int)strlen(title)
    );
}

#ifdef __cplusplus
extern "C" {
    #endif

    // Exported: set title. Returns 1 on success, 0 on failure.
    __attribute__((visibility("default")))
    int rsim_set_window_title(const char* title)
    {
        if (!title || title[0] == '\0') return 0;

        Display* dpy = XOpenDisplay(NULL);
        if (!dpy) return 0;

        pid_t pid = getpid();
        Atom atomNetWmPid = XInternAtom(dpy, "_NET_WM_PID", False);
        Window root = DefaultRootWindow(dpy);

        // If cached window is missing/invalid, clear it and refind.
        if (g_cachedWindow) {
            XWindowAttributes attrs;
            if (XGetWindowAttributes(dpy, g_cachedWindow, &attrs) == 0) {
                g_cachedWindow = 0;
            }
        }

        if (!g_cachedWindow) {
            g_cachedWindow = find_window_by_pid(dpy, root, pid, atomNetWmPid);
        }

        if (!g_cachedWindow) {
            XCloseDisplay(dpy);
            return 0;
        }

        set_title(dpy, g_cachedWindow, title);
        XFlush(dpy);
        XCloseDisplay(dpy);
        return 1;
    }

    // Exported: reset cache (useful if Unity recreates the window after fullscreen/resolution changes).
    __attribute__((visibility("default")))
    void rsim_reset_window_cache(void)
    {
        g_cachedWindow = 0;
    }

    #ifdef __cplusplus
}
#endif
