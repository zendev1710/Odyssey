using System;
using System.Collections.Generic;
using AvaloniaEdit.CodeCompletion;
using Odyssey.ViewModels.Tools;

namespace Odyssey.Views.Tools;
public class OrderDefinition
{
    public string Keyword { get; set; }
    public string Description { get; set; }
    public int MaxParameters { get; set; } // Nombre maximum de paramètres attendus

    // Fonction qui renvoie les suggestions pour ce paramètre (basé sur le contexte)
    // public Func<UnitOrdersViewModel, List<ICompletionData>>[] ParameterProviders { get; set; }
    // On passe toute la ligne pour permettre l'analyse intelligente
    public Func<UnitOrdersViewModel, string[], List<ICompletionData>> DynamicProvider { get; set; }
}
