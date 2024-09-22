﻿using Content.Client.UserInterface.Controls;
using Content.Shared.Radium.Changeling.Components;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.Radium.Changeling.UI;

[GenerateTypedNameReferences]
public sealed partial class ChangelingStorage : FancyWindow
{
    [Dependency] private readonly EntityManager _entityManager = default!;

    public EntityUid Uid;
    private readonly BoundUserInterface _bui;
    private readonly Dictionary<int, KeyValuePair<int, string>> _identitiesOrder = new();
    private int _selectedIdentityServerIndex;

    public ChangelingStorage(BoundUserInterface bui, EntityUid entityUid)
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        ConfirmTransformation.Disabled = true;

        Uid = entityUid;
        _bui = bui;

        Resizable = false;

        IdentitiesList.SelectMode = ItemList.ItemListSelectMode.Single;
        UpdateIdentities(entityUid);

        Title = $"{Loc.GetString("changeling-transform-window-title")} ({IdentitiesList.Count}/7)";

        IdentitiesList.OnItemSelected += OnItemSelected;
        ConfirmTransformation.OnPressed += OnTransformationConfirmed;
    }

    private void OnTransformationConfirmed(BaseButton.ButtonEventArgs args)
    {
        switch (_bui)
        {
            case ChangelingDnaStorageBoundUserInterfaceTransform transform:
                transform.ConfirmTransformation(
                    _entityManager.GetNetEntity(Uid),
                    _selectedIdentityServerIndex);
                break;
            case ChangelingDnaStorageBoundUserInterfaceSting sting:
                sting.ConfirmSting(_entityManager.GetNetEntity(Uid),
                    _selectedIdentityServerIndex);
                break;
        }
    }

    private void OnItemSelected(ItemList.ItemListSelectedEventArgs args)
    {
        if (_identitiesOrder.TryGetValue(args.ItemIndex, out var identity))
            _selectedIdentityServerIndex = identity.Key;
        ConfirmTransformation.Disabled = false;
    }

    public void UpdateIdentities(EntityUid entityUid)
    {
        IdentitiesList.Clear();
        _identitiesOrder.Clear();

        if (!_entityManager.TryGetComponent<ChangelingComponent>(entityUid, out var changeling))
            return;

        var index = 0;
        foreach (var identity in changeling.ClientIdentitiesList)
        {
            IdentitiesList.AddItem($"{identity.Value}");
            _identitiesOrder.Add(index, identity);
            index++;
        }

        Title = $"{Loc.GetString("changeling-transform-window-title")} ({IdentitiesList.Count}/7)";
    }
}