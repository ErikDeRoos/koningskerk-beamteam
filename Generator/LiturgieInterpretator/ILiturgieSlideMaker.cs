// Copyright 2019 door Erik de Roos
using Generator.Database.Models;
using Generator.LiturgieInterpretator.Models;
using System.Collections.Generic;

namespace Generator.LiturgieInterpretator
{

    public interface ILiturgieSlideMaker
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tekstInput"></param>
        /// <param name="masks"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        ITekstNaarSlideConversieResultaat ConverteerNaarSlide(ILiturgieTekstObject tekstInput, LiturgieSettings settings, IEnumerable<LiturgieMapmaskArg> masks = null);
    }
}
