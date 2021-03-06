﻿using Engine;

namespace Collada
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("4 Collada", false, 1280, 720, true, 0, 0))
#else
            using (Game cl = new Game("4 Collada", true, 0, 0, true, 0, 0))
#endif
            {
#if DEBUG
                cl.VisibleMouse = false;
                cl.LockMouse = false;
#else
                cl.VisibleMouse = false;
                cl.LockMouse = true;
#endif

                cl.AddScene<NavmeshTest>();

                cl.Run();
            }
        }
    }
}
