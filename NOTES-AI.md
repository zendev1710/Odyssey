# Notes AI

## Refactoring using services instead of Prism EventAggregator

"Je voudrais améliorer mon code. Mes ViewModels (comme HistoryViewModel) héritent de DocumentToolViewModelBase. Chacun a une propriété Report de type CRDorcuemnt. Or dans mon applciation vbbasée sur Dock Avalonia, by design je ne peux ouvrir qu'un fichier report, (et un CRDocument vide et créé tant qu'un fichier report n'est pas ouvert). Ensuite via Prism eventAggregator, je mets à jour Report quand un document est ouvert à la place du courant, quand le document courant est fermé, etc. Je voudrais qu'il n'y ait qu'une source de vérité, gérée par un service. Peux-tu m'aider ? 
ok. Penses-tu que  l'on pourrait remplacer dans le même plan tous les usages d'eventAggregator Prism de la même façon ? La gestoin de la sélection et un peu particulière : divers types d'éléments peuvent être sélectionnés : une région, un batiment ou un bateau (suppose que la région à laquelle il appartient est sélectionnée), une unité (suppose que la faction et la région auxquelles elle appartient sont sélectionnées). Ou fait-on cela dans un second temps ?
SelectionService était un premier essai (mais non utilisé) pour centraliser la sélection, pour jsutement avoir une source de vérité unique. Un seul élément est sélectionné dans toute l'application et on utilisait Prism EventAggregator pour dispatcher un évenement de changement de sélection.. Juste le service SelectionService en ébauche devrait pas gérer uniquement la séleciton d'une unité mais de tout type d'élément  précisé précédemment.
"

L'état actuel suite à ce qui a déjà été fait :
✅ ActiveDocumentService créé (interface + impl.)
✅ SelectionService extendu
✅ ServiceLocator pas encore mis à jour
❌ DockFactory pas fait
❌ 3 base classes pas faites
❌ MainWindowViewModel pas fait
❌ Tool ViewModels pas faits

on en est aux adaptations de DockFactory.

The goal is to refactor so that ViewModels use ServiceLocator.ActiveDocumentService and ServiceLocator.SelectionService instead of passing their own copies via Prism EventAggregator.

✅ ActiveDocumentService créé (interface + impl.) - done
✅ SelectionService extendu - done
✅ ServiceLocator mis à jour - done (ActiveDocumentService and SelectionService are registered)
❌ DockFactory pas fait - this is where we are
❌ 3 base classes pas faites - DocumentToolViewModelBase, ViewModelBase, ClientAreaDockBase

Now let refactor DocumentToolViewModelBase first. The key changes:
- Remove IEventAggregator field and use ServiceLocator.ActiveDocumentService instead
- Make Report delegate to the service
- Subscribe to ActiveDocumentChanged event from service
- Keep Prism events for SelectionChangeEvent (still needed)


