using System;
using System.Collections.Generic;
using System.IO;

namespace PowerpointGenerater
{
    class Uitlezen_Liturgie
    {
        //bevat strings met tekst voor liederen
        //of bevat pad naar sheets die rechtstreeks ingevoegd mogen gaan worden
        public List<String> inhoud = new List<String>();
        //bevat strings met tekst voor bestandsnamen(Titel bij liederen)
        public List<String> bestandsnamen = new List<String>();
        
        public String mappath = "";
        
        //controle of alle ingevoerde liturgie gevonden kon worden
        public bool AllLiturgieFound = true;
        //indicatie of de liturgie op het liturgiebord moet komen
        public bool Liturgiebord = false;
        
        /// <summary>
        /// Parse de tekst die word ingevoerd naar paths waarop gezocht kan worden
        /// </summary>
        /// <param name="Input">de tekst die is ingevoerd</param>
        /// <param name="DatabasePath">de locatie van de database</param>
        /// <returns></returns>
        public List<String> ParseInputToPaths(String Input, String DatabasePath, List<Mapmask> masks)
        {
            //hoe de string opgedeeld moet worden om er een maplocatie uit te kunnen krijgen
            char[] separators = { ' ', ':' };
            //deel de liturgieregel op
            //string[] Liturgieonderdelen = Input.Split(separators);
            List<string> Liturgieonderdelen = new List<string>();
            //deel de liturgieregel op en filter lege liturgieonderdelen weg
            foreach (string s in Input.Split(separators))
            {
                //als het liturgieonderdeel gevuld is heeft deze toegevoegde waarde voor het resultaat
                if (!s.Equals(""))
                {
                    Liturgieonderdelen.Add(s);
                }
            }
            //maak een nieuwe lijst aan om de verschillende paden in op te slaan
            List<String> paths = new List<String>();
            //een tijdelijk mappad waar de verschillende bestanden gevonden moeten kunnen gaan worden
            String mappath = DatabasePath;
            //correctie
            mappath = mappath.Split(';')[0];
            //Bestandsnaam
            String bestandsnaam = "";

            //zolang er nog onderdelen zijn voor het pad
            for (int i = 0; i < Liturgieonderdelen.Count; i++)
            {
                //om op de juiste mappen uit te komen zijn verschillende afwijkingen qua schrijfwijze mogelijk
                if (Liturgieonderdelen[i].Equals("Ps") || Liturgieonderdelen[i].Equals("PS") || Liturgieonderdelen[i].Equals("ps"))
                {
                    Liturgieonderdelen[i] = "psalm";
                }
                else if (Liturgieonderdelen[i].Equals("Gz") || Liturgieonderdelen[i].Equals("GZ") || Liturgieonderdelen[i].Equals("gz"))
                {
                    Liturgieonderdelen[i] = "gezang";
                }
                else if (Liturgieonderdelen[i].Equals("Lied"))
                {
                    Liturgieonderdelen[i] = "lied";
                }
                //als het het laatste onderdeel bevat van het pad zijn dit de bestandsnamen
                if (!((i + 1) < Liturgieonderdelen.Count))
                {
                    //deel de verschillende bestandsnamen op(er zijn verschillende verzen mogelijk uit één map bijvoorbeeld)
                    string[] tempfiles = Liturgieonderdelen[i].Split(',');
                    //voeg voor ieder bestandsnaam een nieuw pad in
                    foreach (String s in tempfiles)
                    {
                        //als het bestandsnaam leeg is kunnen we deze uitfilteren
                        if (!s.Equals(""))
                        {
                            //voeg een pad naar liturgie toe
                            paths.Add(mappath + @"\" + s);

                            bool maskgevonden = false;
                            foreach (Mapmask mask in masks)
                            {
                                if (mask.RealName == s)
                                {
                                    //voeg de bestandsnamen toe aan de lijst van bestandsnamen voor onderscheid met mask
                                    bestandsnamen.Add(mask.Name);
                                    maskgevonden = true;
                                }
                            }
                            if(!maskgevonden)
                            {
                                //voeg de bestandsnamen toe aan de lijst van bestandsnamen voor onderscheid
                                bestandsnamen.Add(s);
                            }
                        }
                    }
                }
                //in alle andere gevallen gaat het nog om mappen en kunnen we hem linea recta invoegen in het pad
                else
                {
                    mappath += @"\";
                    mappath += Liturgieonderdelen[i];

                    bool maskgevonden = false;
                    foreach (Mapmask mask in masks)
                    {
                        if (mask.RealName == Liturgieonderdelen[i])
                        {
                            bestandsnaam += mask.Name;
                            maskgevonden = true;
                        }
                    }
                    if(!maskgevonden)
                        bestandsnaam += Liturgieonderdelen[i];
                    if (Liturgieonderdelen.Count > 2)
                    {
                        this.mappath = bestandsnaam + ": ";
                    }
                    else
                    {
                        this.mappath = bestandsnaam + "  ";                      
                    }
                    bestandsnaam += " ";
                    //het zijn liederen dus die moeten op het liturgiebord
                    Liturgiebord = true;
                }
            }
            return paths;
        }
        /// <summary>
        /// Zoeken naar liturgie op de opgegeven paden (.txt)
        /// </summary>
        /// <param name="Liturgiepad">de paden waar de liturgieën moet staan</param>
        public void LeesLiturgie(List<String> Liturgiepad)
        {
            //voor elk liturgiepad halen we de bijbehorende tekst
            foreach (String Liturgie in Liturgiepad)
            {
                //probeer eerst powerpoint bestanden
                String path = Liturgie;                
                path += ".pptx";

                //als de sheets niet bestaan
                if (!File.Exists(path))
                {
                    path = Liturgie;
                    path += ".ppt";
                    if (!File.Exists(path))
                    {
                        path = Liturgie;
                        path += ".txt";

                        if (!File.Exists(path))
                        {
                            //niet alle liturgie kan worden gevonden
                            AllLiturgieFound = false;
                            //verwijder de bestandsnaam uit de lijst, omdat het bestand immers niet bestaat
                            bestandsnamen.Remove(Liturgie);
                        }
                        else
                        {
                            //probeer om tekst te lezen van bestand
                            try
                            {
                                //open een filestream naar het gekozen bestand
                                FileStream strm = new FileStream(path, FileMode.Open, FileAccess.Read);

                                //gebruik streamreader om te lezen van de filestream
                                using (StreamReader rdr = new StreamReader(strm))
                                {
                                    //return de liturgie
                                    inhoud.Add(rdr.ReadToEnd());

                                }
                            }
                            //vang errors af en geef een melding dat er iets is fout gegaan
                            catch (Exception)
                            {
                                System.Windows.Forms.MessageBox.Show("Fout tijdens openen bestand", "Bestand error",
                                                 System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                            }
                        }
                    }
                    else
                    {
                        inhoud.Add(path);
                    }
                }
                //anders zijn het sheets die rechtstreeks ingevoegd mogen gaan worden
                else
                {
                    inhoud.Add(path);
                }                               
            }
        }
    }
}
