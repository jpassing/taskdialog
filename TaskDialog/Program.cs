//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//

using System;
using System.Runtime.InteropServices;

namespace TaskDialog
{
    class Program
    {
        static void ShowMessage()
        {
            var config = new UnsafeNativeMethods.TASKDIALOGCONFIG()
            {
                cbSize = (uint)Marshal.SizeOf(typeof(UnsafeNativeMethods.TASKDIALOGCONFIG)),
                hwndParent = new IntPtr(0),
                dwFlags = 0,
                dwCommonButtons = UnsafeNativeMethods.TASKDIALOG_COMMON_BUTTON_FLAGS.TDCBF_OK_BUTTON,
                pszWindowTitle = "This is an error message",
                MainIcon = UnsafeNativeMethods.TD_ERROR_ICON,
                pszMainInstruction = "This is the instruction",
                pszContent = "This is the content",
                pszExpandedInformation = "These are the details"
            };

            UnsafeNativeMethods.TaskDialogIndirect(
                ref config,
                out int buttonPressed,
                out int radioButtonPressed,
                out bool verificationFlagPressed);
        }

        static void ShowChoice()
        {
            // The options to show.
            var options = new IntPtr[] {
                Marshal.StringToHGlobalUni("Option 0"),
                Marshal.StringToHGlobalUni("Option 1"),
                Marshal.StringToHGlobalUni("Option 2")
            };

            // Wrap each option by a TASKDIALOG_BUTTON_RAW structure and 
            // marshal them one by one into a native memory buffer.
            var buttonsBuffer = Marshal.AllocHGlobal(
                Marshal.SizeOf<UnsafeNativeMethods.TASKDIALOG_BUTTON_RAW>() * options.Length);

            var currentButton = buttonsBuffer;
            for (int i = 0; i < options.Length; i++)
            {
                Marshal.StructureToPtr<UnsafeNativeMethods.TASKDIALOG_BUTTON_RAW>(
                    new UnsafeNativeMethods.TASKDIALOG_BUTTON_RAW()
                    {
                        nButtonID = i,
                        pszButtonText = options[i]
                    }, 
                    currentButton, 
                    false);
                currentButton += Marshal.SizeOf<UnsafeNativeMethods.TASKDIALOG_BUTTON_RAW>();
            }

            try
            {
                var config = new UnsafeNativeMethods.TASKDIALOGCONFIG()
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(UnsafeNativeMethods.TASKDIALOGCONFIG)),
                    hwndParent = new IntPtr(0),
                    dwFlags = UnsafeNativeMethods.TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS,
                    dwCommonButtons = UnsafeNativeMethods.TASKDIALOG_COMMON_BUTTON_FLAGS.TDCBF_OK_BUTTON,
                    pszWindowTitle = "Make a decition",
                    MainIcon = UnsafeNativeMethods.TD_INFORMATION_ICON,
                    pszMainInstruction = "This is the instruction",
                    pszContent = "This is the content",
                    pButtons = buttonsBuffer,
                    cButtons = (uint)options.Length,
                    pszExpandedInformation = "These are the details"
                };

                UnsafeNativeMethods.TaskDialogIndirect(
                    ref config,
                    out int buttonPressed,
                    out int radioButtonPressed,
                    out bool verificationFlagPressed);

                Console.WriteLine($"Pressed: {buttonPressed}");
            }
            finally
            {
                foreach (var option in options)
                {
                    Marshal.FreeHGlobal(option);
                }

                Marshal.FreeHGlobal(buttonsBuffer);
            }
        }

        static void Main(string[] args)
        {
            ShowMessage();
            ShowChoice();
        }
    }
}
