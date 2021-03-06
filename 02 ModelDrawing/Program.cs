﻿using Engine;

namespace ModelDrawing
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            using (Game cl = new Game("2 ModelDrawing", false, 1000, 562, true, 0, 4))
#else
            using (Game cl = new Game("2 ModelDrawing", true, 0, 0, true, 0, 0))
#endif
            {
#if DEBUG
                cl.VisibleMouse = false;
                cl.LockMouse = false;
#else
                cl.VisibleMouse = false;
                cl.LockMouse = true;
#endif
                
                cl.AddScene<TestScene>();

                cl.Run();
            }
        }
    }
}
