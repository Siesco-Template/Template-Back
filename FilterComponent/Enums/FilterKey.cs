namespace FilterComponent.Enums
{
    /// <summary>
    /// Daxil edilən Filterin column-un ne oldugunu bildirir(Back-de hec bir islevi yoxdur Front üçündür)
    /// </summary>
    public enum FilterKey : byte
    {
        //text , number , numberInterval , select , multiSelect , date , dateInterval

        Text = 1,
        Number = 2,
        NumberInterval = 3,
        Select = 4,
        MultiSelect = 5,
        Date = 6,
        DateInterval = 7
    }
}