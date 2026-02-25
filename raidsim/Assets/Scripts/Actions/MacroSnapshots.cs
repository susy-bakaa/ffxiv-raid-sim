// SPDX-License-Identifier: GPL-3.0-only
// This file is part of ffxiv-raid-sim. Linking with the Unity runtime
// is permitted under the Unity Runtime Linking Exception (see LICENSE).
using System.Collections.Generic;
using static dev.susybaka.raidsim.Core.GlobalData;

namespace dev.susybaka.raidsim.Actions
{
    public static class MacroSnapshots
    {
        public static MacroLibrarySnapshot CreateSparseSnapshot(MacroLibrary library)
        {
            var list = new List<MacroSlotSave>();

            for (int i = 0; i < MacroLibrary.Count; i++)
            {
                var e = library.Get(i);
                if (!e.isValid)
                    continue;
                list.Add(new MacroSlotSave { index = i, entry = e });
            }

            return new MacroLibrarySnapshot
            {
                version = 1,
                slots = list.ToArray()
            };
        }

        public static void ApplySnapshotOverwrite(MacroLibrary library, MacroLibrarySnapshot snap)
        {
            library.ClearAll(); // Clear the array without update notifications, since we'll be doing a bulk update after this.

            if (snap?.slots == null)
                return;

            for (int i = 0; i < snap.slots.Length; i++)
            {
                var s = snap.slots[i];
                if (s.index < 0 || s.index >= MacroLibrary.Count)
                    continue;
                library.Set(s.index, s.entry);
            }

            library.NotifyBulkChanged();
        }
    }
}