﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Messenger_Mobile_App.Services
{
    public interface IDataStore<T> // this data store use string for id
    {
        Task<bool> AddItemAsync(T item);
        Task<bool> UpdateItemAsync(T item);
        Task<bool> DeleteItemAsync(string id);
        Task<T> GetItemAsync(string id);
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
    }
}
