﻿using Engine;

namespace Terrain
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("6 Terrain", false, 1280, 720, true, 0, 4))
#else
            using (Game cl = new Game("6 Terrain", true, 0, 0, true, 0, 0))
#endif
            {
#if DEBUG
                cl.VisibleMouse = false;
                cl.LockMouse = false;
#else
                cl.VisibleMouse = false;
                cl.LockMouse = true;
#endif

                cl.AddScene<TestScene3D>();

                cl.Run();
            }
        }
    }
}
