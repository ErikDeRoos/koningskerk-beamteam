﻿// Copyright 2016 door Erik de Roos
namespace PowerpointGenerator.Genereren
{
    public interface ICompRegistration
    {
        string[] Liturgie { get; set; }
        string Voorganger { get; set; }
        string Collecte1e { get; set; }
        string Collecte2e { get; set; }
        string[] Lezen { get; set; }
        string[] Tekst { get; set; }
    }
}
