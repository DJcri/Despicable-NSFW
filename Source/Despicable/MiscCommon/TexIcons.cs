using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Despicable
{
    [StaticConstructorOnStartup]
    public static class TexIcons
    {
        // [-= Face Parts Module]
        public static readonly Texture2D FaceGizmo = ContentFinder<Texture2D>.Get("UI/FaceParts/FaceGizmo");

        // [-= Lovin Module =-]
        public static readonly Texture2D Vaginal = ContentFinder<Texture2D>.Get("UI/Interaction/vaginal");
        public static readonly Texture2D Oral = ContentFinder<Texture2D>.Get("UI/Interaction/oral");
        public static readonly Texture2D Anal = ContentFinder<Texture2D>.Get("UI/Interaction/anal");

        // [-= Hero Module =-]
        public static readonly Texture2D HeroGizmo = ContentFinder<Texture2D>.Get("UI/Karma/HeroGizmo");
        public static readonly Texture2D GoodKarma = ContentFinder<Texture2D>.Get("UI/Karma/GoodKarma");
        public static readonly Texture2D BadKarma = ContentFinder<Texture2D>.Get("UI/Karma/BadKarma");
        // Meter
        public static readonly Texture2D Marker = ContentFinder<Texture2D>.Get("UI/Karma/Marker");
        // Good karma
        public static readonly Texture2D Proselytize = ContentFinder<Texture2D>.Get("UI/Abilities/Proselytize");
        public static readonly Texture2D PetAnimal = ContentFinder<Texture2D>.Get("UI/Abilities/PetAnimal");
        public static readonly Texture2D PepTalk = ContentFinder<Texture2D>.Get("UI/Abilities/PepTalk");
        public static readonly Texture2D Uplift = ContentFinder<Texture2D>.Get("UI/Abilities/Uplift");
        // Bad karma
        public static readonly Texture2D OperantTraining = ContentFinder<Texture2D>.Get("UI/Abilities/OperantTraining");
        public static readonly Texture2D Oversee = ContentFinder<Texture2D>.Get("UI/Abilities/Oversee");
        public static readonly Texture2D Torture = ContentFinder<Texture2D>.Get("UI/Abilities/Torture");
        public static readonly Texture2D FearMonger = ContentFinder<Texture2D>.Get("UI/Abilities/FearMonger");
    }
}