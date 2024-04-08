using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BankSim.Component;

public class CustomValidator : ComponentBase
{
    private ValidationMessageStore _messageStore;
    [CascadingParameter]
    public EditContext editContext { get; set; }

    protected override void OnInitialized()
    {
        if (editContext == null)
        {
            throw new InvalidOperationException("To use validator component your razor page should have the edit component");
        }

        _messageStore = new ValidationMessageStore(editContext);
        editContext.OnValidationRequested += (s, e) => _messageStore.Clear();
        editContext.OnFieldChanged += (s, e) => _messageStore.Clear(e.FieldIdentifier);
    }

    public void DisplayErrors(Dictionary<string, List<string>> errors)
    {
        foreach (var error in errors)
        {
            _messageStore.Add(editContext.Field(error.Key), error.Value);
        }

        editContext.NotifyValidationStateChanged();
    }


    public void ClearErrors()
    {
        _messageStore.Clear();
    }
}
