// Copyright 2020 door Erik de Roos
using Generator.Database.Models;
using System;
using System.Collections.Generic;

namespace mppt
{
    public interface IBuilder
    {
        Action<int, int, float> Voortgang { set; }
        Action<Status, string, int?> StatusWijziging { set; }

        void PreparePresentation(IEnumerable<ISlideOpbouw> liturgie, IBuilderBuildSettings buildSettings, IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, string opslaanAls);
        void GeneratePresentation();
        void ProbeerStop();
        void ForceerStop();
    }

    public enum Status
    {
        Gestart,
        StopFout,
        StopGoed,
    }
}