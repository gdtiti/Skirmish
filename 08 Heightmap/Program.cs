﻿using Engine;

namespace Heightmap
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("8 Heightmap", false, 800, 450, false, 0, 0))
#else
            using (Game cl = new Game("8 Heightmap", true, 0, 0, true, 0, 0))
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
