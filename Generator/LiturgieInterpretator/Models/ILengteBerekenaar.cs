// Copyright 2017 door Erik de Roos
namespace Generator.LiturgieInterpretator.Models
{
    public interface ILengteBerekenaar
    {
        float VerbruiktPercentageVanRegel(string tekst, bool needsPad);
    }
}
