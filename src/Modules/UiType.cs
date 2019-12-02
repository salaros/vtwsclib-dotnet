namespace Salaros.vTiger.WebService
{
    public enum UiType
    {
        Unknown = 0,

        /// <summary>
        /// A single line text field. Changes color when selected.
        /// </summary>
        Text = 1,

        /// <summary>
        /// Same as uitype 1, but this is a mandatory field. A star is present before the field label. Changes color when selected.
        /// </summary>
        TextMandatory = 2,

        /// <summary>
        /// An non-editable single line text field which displays auto-generated and incremental input, like invoice number. It is a mandatory field.
        /// A star is present before the field label. Changes color when selected.
        /// </summary>
        GeneratedReadonlyText = 3,

        /// <summary>
        /// Used for generating Auto number of configured type in any module.
        /// </summary>
        AutoIncrement = 4,

        /// <summary>
        /// Date field. Contains a link to "jsCalendar" with it which can be used to fill it in. Also displays he current users date format below it.
        /// Takes inut date based on the current users date format. Does not allow to enter invalid date like 30th February. Does not change color on selection.
        /// Mostly used to take start date inputs.
        /// </summary>
        Date = 5,

        /// <summary>
        /// This is a time and date field. It allows to enter time using drop-downs and date using a link to "jscalendar".
        /// The validity of date and time is checked before entry. It is mandatory that the entered date is greater than or equal to the current date.
        /// Does not change color on selection.
        /// </summary>
        DateTime = 6,

        /// <summary>
        /// A single line text field which is used to take numeric input primarily.
        /// Changes color on selection.
        /// </summary>
        Number = 7,

        /// <summary>
        /// JSON array is stored and the value when consumed will be comma separated strings.
        /// </summary>
        ListOfStrings = 8,

        /// <summary>
        /// A single line text field which is used to accept percentage inputs from users. Checks if the input is less than 100 or not and gives and error otherwise.
        /// Changes color on selection.
        /// </summary>
        Percent = 9,

        /// <summary>
        /// To create an input type of 'Linked Entity' wherein a field can be linked to one of the entity from multiple modules (e.g. Member of field) -> Introduced by vtlib
        /// </summary>
        Entity = 10,

        /// <summary>
        /// A single line text field. Has no checks for the validity of input data. Changes color on selection.
        /// </summary>
        TextLine = 11,

        /// <summary>
        /// Email id field which stores the single email id (from email address), when mail is sent from the system
        /// </summary>
        Organization = 12,

        /// <summary>
        /// Email id field which stores the single email id (from email address), when mail is sent from the system
        /// </summary>
        EmailAddress = 13,

        /// <summary>
        /// The picklist / drop-down
        /// </summary>
        Picklist = 15,

        /// <summary>
        /// A drop-down combo that accepts input from the value selected in it. The values in the drop-down vary from module to module and Role-based.
        /// </summary>
        PicklistMandatory = 16,

        /// <summary>
        /// URL. Single line text field which is used to accept the names of websites from the users. Does not check for the validity of input.
        /// Changes color on selection.
        /// </summary>
        Url = 17,

        /// <summary>
        /// Text-area used for accepting large inputs like "Description", "Solutions" etc.
        /// Changes color on selection.
        /// </summary>
        TextAreaSmall = 19,

        /// <summary>
        /// Same as uitype 19, but a mandatory field, i.e. it has to be filled and there is a star present before the fieldlabel.
        /// Changes color on selection.
        /// </summary>
        TextAreaSmallMandatory = 20,

        /// <summary>
        /// Text-area sized around five lines. Used to take small details like "Street Address" from user as input.
        /// Changes color on selection.
        /// </summary>
        TextArea = 21,

        /// <summary>
        /// A text-area which is used to accept the "Title" field in some modules. It is a mandatory field. A star is present before the fieldlabel.
        /// Changes color on selection.
        /// </summary>
        TextAreaMandatory = 22,

        /// <summary>
        /// Date field. Same as uitype 5, but mostly used to take end date inputs.
        /// </summary>
        DateEnd = 23,

        /// <summary>
        /// Text-area sized around five lines. Primarily used to take small details like "Billing Address" from user as input.
        /// When a contact is selected, then if the user consents, the billing address is filled automatically using the contact address as billing address.
        /// Is a mandatory field. A star is present before the textlabel. Changes color on selection.
        /// </summary>
        TextAreaAutofilled = 24,

        /// <summary>
        /// Email Status Tracking (Used to count the number of times an email is opened).
        /// This is a special uitype, value for which is computed based on the values of the other table.
        /// </summary>
        EmailTrack = 25,

        /// <summary>
        /// Documents folder
        /// </summary>
        DocumentFolder = 26,

        /// <summary>
        /// File type information (Internal/External).
        /// This uitype is special as it gives a picklist selection on the left side where label appears and based on which the input type for the value changes.
        /// </summary>
        File = 27,

        /// <summary>
        /// Field for filename holder (which was previously merged with another uitype).
        /// Now this field exists independent of the other uitype, but its type varies based on the value of the other uitype
        /// </summary>
        FileName = 28,

        /// <summary>
        /// This consists of three drop-downs which are used to set the reminder time in case of any activity creation.
        /// </summary>
        TimeLeft = 30,

        /// <summary>
        /// This consists of three drop-downs which are used to set the reminder time in case of any activity creation.
        /// </summary>
        TextAreaCombo = 33,

        /// <summary>
        /// Popup select box for account and contact addresses.
        /// Used to select an account from a popup window.
        /// </summary>
        Account = 51,

        /// <summary>
        /// A drop-down combo that accepts input from the value selected in it.
        /// The input is the name of handler (like admin, standarduser etc.) for the entity being created.
        /// </summary>
        Username = 52,

        /// <summary>
        /// Combination of a drop-down combo and a radiobutton that accepts input from the value selected in the combo.
        /// The value of the radiobutton, in turn, decides the values in the combo. The input is the name of the user or group to which an activity is assigned to.
        /// </summary>
        User = 53,

        /// <summary>
        /// This uitype provides a combination of Salutation and Firstname.
        /// The Salutation field is a drop-down while the Firstname field is a single line text field which changes its color on selection.
        /// </summary>
        Salutation = 55,

        /// <summary>
        /// A checkbox which takes input in the form of yes or no.
        /// </summary>
        Checkbox = 56,

        /// <summary>
        /// A single line uneditable text field. Takes its input from the link provided with it. Used to select a contact from a popup window.
        /// Contains a link which can be used to clear previous value.
        /// Also it contains a link to a popup which adds a new contact. Does not change color on selection.
        /// </summary>
        Contact = 57,

        /// <summary>
        /// Campaign popup select box
        /// </summary>
        Campaign = 58,

        /// <summary>
        /// Product non-editable capture, popup picklist
        /// </summary>
        Product = 59,

        /// <summary>
        /// Attachments, file selection box
        /// </summary>
        Attachment = 61,

        /// <summary>
        /// Duration minutes picklist - different typeofdata for the tab_id: 9 and 16 ???
        /// </summary>
        Minutes = 63,

        Activity = 66,

        Ticket = 68,

        /// <summary>
        /// Products attachments
        /// </summary>
        ProductAttachment = 69,

        /// <summary>
        /// Date (for the created and modified date & time)
        /// </summary>
        DateCreatedModified = 70,

        Currency = 71,

        /// <summary>
        /// Popup select box for Accounts, mandatory entry [Calls JS function to auto fill billing and shipping address fields.
        /// Contact pop-up limited to only contacts related to the selected Account]
        /// </summary>
        AccountContacts = 73,

        /// <summary>
        /// Vendor name
        /// </summary>
        Vendor = 75,

        /// <summary>
        /// Potential popup picklist
        /// </summary>
        Potential = 76,

        /// <summary>
        /// Picklist for secondary username entries
        /// </summary>
        UserAssigned = 77,

        /// <summary>
        /// Quote popup picklist
        /// </summary>
        Quote = 78,

        /// <summary>
        /// Sales order popup picklist
        /// </summary>
        SalesOrder = 80,

        /// <summary>
        /// Vendor name, mandatory entry
        /// </summary>
        VendorMandatory = 81,

        /// <summary>
        /// Tax in Inventory
        /// </summary>
        Tax = 83,

        /// <summary>
        /// Role name popup picklist, mandatory entry
        /// </summary>
        Role = 98,

        /// <summary>
        /// Password, mandatory entry
        /// </summary>
        Password = 99,

        /// <summary>
        /// User capture popup picklist
        /// </summary>
        UserReportsTo = 101,

        /// <summary>
        /// EMail, mandatory entry
        /// </summary>
        EmaiAddresslMandatory = 104,

        /// <summary>
        /// User image
        /// </summary>
        UserImage = 105,

        /// <summary>
        /// Text box, mandatory entry
        /// </summary>
        TextMandatory2 = 106,

        /// <summary>
        /// Non editable picklist
        /// </summary>
        PicklistReadonly = 115,

        /// <summary>
        /// Currency in user details
        /// </summary>
        UserCurrency = 116,

        /// <summary>
        /// Currency in modules ??? - missing entries at http://wiki.vtiger.com/index.php/Ui_types
        /// </summary>
        Currency2 = 117,

        /// <summary>
        /// Admin toggle, checkbox
        /// </summary>
        UserIsAdmin = 156,

        /// <summary>
        /// In leads and contacts module, last name is mandatory but first name is not.
        /// So when first name is disabled for the profile, the salutation gets handled and added for the last name using this uitype.
        /// </summary>
        SalutationInLeads = 255,

        /// <summary>
        /// Email, Popup picklist
        /// </summary>
        EmailEntity = 357,
    }
}
