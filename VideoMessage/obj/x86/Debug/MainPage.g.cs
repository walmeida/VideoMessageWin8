﻿

#pragma checksum "C:\Users\Victor\development\repository\vtx\VideoMessageWin8\VideoMessage\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B3B1BFC45822355BCFCB0FE47BDC13C9"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VideoMessage
{
    partial class MainPage : global::VideoMessage.Common.LayoutAwarePage, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 57 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.btnStartStopRecord_Click;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 58 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.PickAContactButton_Click;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 59 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.btnPlayMensagem_Click;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 50 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.SendButton_Click;
                 #line default
                 #line hidden
                break;
            case 5:
                #line 36 "..\..\..\MainPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.GoBack;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


