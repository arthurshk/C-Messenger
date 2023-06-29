using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CommonLibrary.Requests;

public class LoginRequest : IDataContent
{
    public Client Me { get; set; }

    public Data ToData()
    {
        return new Data
        {
            Type = DataType.LoginRequest,
            Content = JsonSerializer.Serialize(this)
        };
    }
}


/*
 * Варіант 1
 * Зробити свій проект з аналогічним функціоналом
 * 
 * Варіант 2
 * Зробити по цьому проєкту десктоп версію клієнта
 * WinForm / WPF
 * 
 * Варіант 3
 * Додати функціонал авторизацію по логін та паролю
 */ 