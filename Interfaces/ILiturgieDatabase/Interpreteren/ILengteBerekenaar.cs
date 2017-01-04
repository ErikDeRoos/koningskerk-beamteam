// Copyright 2017 door Erik de Roos
namespace ILiturgieDatabase
{
    public interface ILengteBerekenaar
    {
        float VerbruiktPercentageVanRegel(string tekst, bool needsPad);
    }
}
