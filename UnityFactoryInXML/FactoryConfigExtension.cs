// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
// https://bitbucket.org/ctavares/unityfactoryconfig/overview

using Microsoft.Practices.Unity.Configuration;

namespace Unity.FactoryConfig
{
    public class FactoryConfigExtension : SectionExtension
    {
        public override void AddExtensions(SectionExtensionContext context)
        {
            context.AddElement<FactoryElement>("factory");
        }
    }
}