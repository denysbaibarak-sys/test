using System;

public class Validator
{
    public void ValidateRestaurant(Restaurant r)
    {
        if (r == null)
            throw new ArgumentException("Restaurant is null");

        if (string.IsNullOrWhiteSpace(r.Name))
            throw new ArgumentException("Restaurant name is empty");

        if (r.Menu == null)
            throw new ArgumentException("Menu is null");
    }
}