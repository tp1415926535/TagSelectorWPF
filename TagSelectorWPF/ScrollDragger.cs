﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace TagSelectorWPF
{
    /// <summary>
    /// Can drag element in scrollViewer
    /// from "https://stackoverflow.com/a/74248141/15891701"
    /// </summary>
    internal class ScrollDragger
    {
        private readonly ScrollViewer _scrollViewer;
        private readonly UIElement _content;
        private readonly Cursor _dragCursor = Cursors.ScrollWE;
        private double _scrollMouseX;
        private double _scrollMouseY;
        private int _updateCounter = 0;

        public ScrollDragger(UIElement content, ScrollViewer scrollViewer)
        {
            _scrollViewer = scrollViewer;
            _content = content;

            content.MouseLeftButtonDown += scrollViewer_MouseLeftButtonDown;
            content.PreviewMouseMove += scrollViewer_PreviewMouseMove;
            content.PreviewMouseLeftButtonUp += scrollViewer_PreviewMouseLeftButtonUp;
        }

        private void scrollViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Capture the mouse, reset counter, switch to hand cursor to indicate dragging
            _content.CaptureMouse();
            _updateCounter = 0;
            _scrollViewer.Cursor = _dragCursor;
        }

        private void scrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_content.IsMouseCaptured)
            {
                _updateCounter++;

                // Skip dragging on the first PreviewMouseMove event after the left mouse button goes down. It actually triggers two of these and this ignores both, preventing jumping.
                if (_updateCounter <= 1)
                {
                    // Grab starting mouse offset relative to scroll viewer, used to calculate first delta
                    _scrollMouseY = e.GetPosition(_scrollViewer).Y;
                    _scrollMouseX = e.GetPosition(_scrollViewer).X;
                    return;
                }

                // Calculate new vertical offset then scroll to it
                var newVOff = HandleMouseMoveAxisUpdateScroll(_scrollViewer.VerticalOffset, ref _scrollMouseY, e.GetPosition(_scrollViewer).Y, _scrollViewer.ScrollableHeight);
                _scrollViewer.ScrollToVerticalOffset(newVOff);

                // Calculate new horizontal offset and scroll to it
                var newHOff = HandleMouseMoveAxisUpdateScroll(_scrollViewer.HorizontalOffset, ref _scrollMouseX, e.GetPosition(_scrollViewer).X, _scrollViewer.ScrollableWidth);
                _scrollViewer.ScrollToHorizontalOffset(newHOff);
            }
        }

        private double HandleMouseMoveAxisUpdateScroll(double offsetStart, ref double oldScrollMouse, double newScrollMouse, double scrollableMax)
        {
            // How far does the user want to drag since the last update?
            var mouseDelta = oldScrollMouse - newScrollMouse;

            // Add mouse delta to current scroll offset to get the new expected scroll offset
            var newScrollOffset = offsetStart + mouseDelta;

            // Keep the scroll offset from going off the screen
            var newScrollOffsetClamped = Math.Min(Math.Max(newScrollOffset, 0), scrollableMax);

            // Save the current mouse position in scroll coordinates so that we'll have it for next update
            oldScrollMouse = newScrollMouse;

            return newScrollOffsetClamped;
        }

        private void scrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _content.ReleaseMouseCapture();
            _updateCounter = 0; // Reset counter, used to prevent jumping at start of drag
            _scrollViewer.Cursor = null;
        }

        public void Unload()
        {
            _content.MouseLeftButtonDown -= scrollViewer_MouseLeftButtonDown;
            _content.PreviewMouseMove -= scrollViewer_PreviewMouseMove;
            _content.PreviewMouseLeftButtonUp -= scrollViewer_PreviewMouseLeftButtonUp;
        }
    }
}
