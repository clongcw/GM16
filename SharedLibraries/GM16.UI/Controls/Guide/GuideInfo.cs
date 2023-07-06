﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GM16.UI.Controls.Guide
{
    public class GuideInfo
    {
        public GuideInfo(string title, string content, string buttonContent = "下一步",
            FrameworkElement? targetControl = null,
            int minWidth = 220,
            int minHeight = 70)
        {
            Title = title;
            Content = content;
            ButtonContent = buttonContent;
            TargetControl = targetControl;
            MinWidth = minWidth;
            MinHeight = minHeight;
        }

        public FrameworkElement? TargetControl { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public string ButtonContent { get; set; }
        public int MinWidth { get; set; }
        public int MinHeight { get; set; }
    }
}
