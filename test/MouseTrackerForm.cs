using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test
{
    internal class MouseTrackerForm : Form
    {
        private int lastX;
        private int lastY;
        static int totalX = 0;
        static int totalY = 0;

        public MouseTrackerForm()
        {
            MouseMove += MouseTrackerForm_MouseMove;
        }

        private void MouseTrackerForm_MouseMove(object sender, MouseEventArgs e)
        {
            int currentX = e.X;
            int currentY = e.Y;

            if (lastX != 0 && lastY != 0 && (currentX != lastX || currentY != lastY))
            {
                int deltaX = currentX - lastX;
                int deltaY = currentY - lastY;

                totalX += deltaX;
                totalY += totalY;

                Console.WriteLine($"DeltaX: {totalX}, DeltaY: {totalY}");
            }

            lastX = currentX;
            lastY = currentY;
        }
    }
}
