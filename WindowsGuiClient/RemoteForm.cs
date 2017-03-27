﻿using Common;
using Common.EventArgs.Network;
using Network.Messages.Connection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsGuiClient
{
    public partial class RemoteForm : Form
    {
        float Ratio;
        Rectangle Bounds;

        String SystemId { get; set; }
        private Pen pen = new Pen(Color.Magenta, 2.0f);

        public ClientThread Manager;

        delegate void SetDrawingAreaHeightCallback(int Height);
        delegate void DrawImageCallback(Image Image, Rectangle Bounds);

        public RemoteForm(String RemoteId)
        {
            this.SystemId = RemoteId;

            InitializeComponent();
   
        }

        protected void setDrawingAreaHeight(int height)
        {
            drawingArea1.Height = height;
            drawingArea1.BackColor = Color.Azure;
        }

        protected void drawImage(Image image, Rectangle bounds)
        {
            drawingArea1.Draw(image, bounds); ;
        }


        protected void OnScreenshotReceive(object sender, ScreenshotReceivedEventArgs e)
        {
            if (Bounds.IsEmpty)
            {
                Bounds = e.Bounds;
            }

            Ratio = (float)this.drawingArea1.Width / (float)Bounds.Width;
            
            if (this.drawingArea1.InvokeRequired)
            {
                SetDrawingAreaHeightCallback d = new SetDrawingAreaHeightCallback(setDrawingAreaHeight);
                this.Invoke(d, new object[] { (int)((float)Bounds.Height * Ratio) });
            }
            else
            {
                setDrawingAreaHeight((int)((float)Bounds.Height * Ratio));
            }

            if (e.Nothing)
			{
                Thread.Sleep(100);
				Manager.Manager.sendMessage(new RequestScreenshotMessage { HostSystemId = this.SystemId, ClientSystemId = Manager.Manager.SystemId });
				return;
			}

            if (e.SystemId == this.SystemId)
            {
                using (var stream = new MemoryStream(e.Image))
                {
                    Image image = Image.FromStream(stream);

                    /*var gfx = drawingArea1.CreateGraphics();
                    gfx.DrawLine(pen, new Point(e.Bounds.X, e.Bounds.Y), new Point(e.Bounds.X + e.Bounds.Width, e.Bounds.Y));
                    gfx.DrawLine(pen, new Point(e.Bounds.X + e.Bounds.Width, e.Bounds.Y), new Point(e.Bounds.X + e.Bounds.Width, e.Bounds.Y + e.Bounds.Y));
                    gfx.DrawLine(pen, new Point(e.Bounds.X + e.Bounds.Width, e.Bounds.Y + e.Bounds.Y), new Point(e.Bounds.X, e.Bounds.Y + e.Bounds.Y));
                    gfx.DrawLine(pen, new Point(e.Bounds.X, e.Bounds.Y + e.Bounds.Y), new Point(e.Bounds.X, e.Bounds.Y));
                    gfx.Dispose();*/

                    if (this.drawingArea1.InvokeRequired)
                    {
                        DrawImageCallback d = new DrawImageCallback(drawImage);
                        this.Invoke(d, new object[] { image, e.Bounds });
                    }
                    else
                    {
                        drawImage(image, e.Bounds);
                    }
                    
                    
                }

                Thread.Sleep(100);
                Manager.Manager.sendMessage(new RequestScreenshotMessage { HostSystemId = this.SystemId, ClientSystemId = Manager.Manager.SystemId });
            }
        }

        internal void setManager(ClientThread manager)
        {
            this.Manager = manager;

            Manager.ClientListener.OnScreenshotReceived += OnScreenshotReceive;

            Manager.Manager.sendMessage(new RequestScreenshotMessage { HostSystemId = this.SystemId, ClientSystemId = Manager.Manager.SystemId, Fullscreen = true });
        }

      

        private void drawingArea1_Click(object sender, EventArgs e)
        {

        }

        private void drawingArea1_MouseMove_1(object sender, MouseEventArgs e)
        {
            Manager.Manager.sendMessage(new Network.Messages.Connection.OneWay.MouseMoveMessage { ClientSystemId = Manager.Manager.SystemId, HostSystemId = this.SystemId, X = (e.X / Ratio), Y = (e.Y / Ratio) });
        }

        private void drawingArea1_MouseClick(object sender, MouseEventArgs e)
        {
            Manager.Manager.sendMessage(new Network.Messages.Connection.OneWay.MouseClickMessage{ ClientSystemId = Manager.Manager.SystemId, HostSystemId = this.SystemId, X = (e.X / Ratio), Y = (e.Y / Ratio) });
        }
    }
}
