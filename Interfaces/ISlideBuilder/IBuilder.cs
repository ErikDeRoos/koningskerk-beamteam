﻿// Copyright 2016 door Erik de Roos
using System;
using System.Collections.Generic;
using ILiturgieDatabase;

namespace ISlideBuilder
{
    public interface IBuilder : IDisposable
    {
        Action<int, int, int> Voortgang { set; }
        Action<Status, string, int?> StatusWijziging { set; }

        void PreparePresentation(IEnumerable<ILiturgieRegel> liturgie, IBuilderBuildSettings buildSettings, IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, string opslaanAls);
        void GeneratePresentation();
        void Stop();
    }

    public enum Status
    {
        Gestart,
        StopFout,
        StopGoed,
    }
}