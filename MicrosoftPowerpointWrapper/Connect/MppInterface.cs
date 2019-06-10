// Copyright 2019 door Erik de Roos
using System;
using System.Collections.Generic;

namespace mppt.Connect
{
    public interface IMppApplication : IDisposable
    {
        void MinimizeInterface();
        IMppPresentatie Open(string bestandsnaam, bool metWindow = true);
    }

    public interface IMppPresentatie : IDisposable
    {
        IMppSlide EersteSlide();
        IEnumerable<IMppSlide> AlleSlides();
        int SlidesKopieNaarPresentatie(IEnumerable<IMppSlide> slides, int retryCount = 3);
        void OpslaanAls(string bestandsnaam);
    }

    public interface IMppSlide
    {
        IEnumerable<IMppShape> Shapes();
        void CopyToClipboard();
    }

    public interface IMppShape { }

    public interface IMppShapeTextbox : IMppShape
    {
        string Text { get; set; }
    }

    public interface IMppShapeTable : IMppShape
    {
        string GetTitelFromFirstRowCell();
        void SetRowsContent(IEnumerable<IMppShapeTableContent> content);
        void TrimRows();
    }

    public interface IMppShapeTableContent
    {
        int Index { get; }
    }
}