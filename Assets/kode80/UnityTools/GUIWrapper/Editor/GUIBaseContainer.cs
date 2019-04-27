﻿//  Copyright (c) 2016, Ben Hopkins (kode80)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
//  
//  1. Redistributions of source code must retain the above copyright notice, 
//     this list of conditions and the following disclaimer.
//  
//  2. Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
//  
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
//  EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
//  MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
//  THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
//  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT 
//  OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
//  HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
//  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
//  EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;

namespace kode80.GUIWrapper {
    public class GUIBaseContainer : GUIBase {
        private List<GUIBase> _children;

        protected virtual bool areChildrenHidden { get { return false; } }

        public GUIBaseContainer() {
            _children = new List<GUIBase>();
        }

        public GUIBase Add(GUIBase child) {
            _children.Add(child);
            return child;
        }

        public void Remove(GUIBase child) {
            _children.Remove(child);
        }

        public void RemoveAll() {
            _children.Clear();
        }

        protected override void CustomOnGUI() {
            BeginContainerOnGUI();

            if (areChildrenHidden == false)
            {
                foreach (GUIBase gui in _children)
                {
                    gui.OnGUI();
                }
            }

            EndContainerOnGUI();
        }

        protected virtual void BeginContainerOnGUI() {
            // Subclasses implement 'opening' container GUI code here
        }

        protected virtual void EndContainerOnGUI() {
            // Subclasses implement 'closing' container GUI code here
        }
    }
}