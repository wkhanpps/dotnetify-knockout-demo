﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Newtonsoft.Json;
using Domain;
using Domain.Service.Interfaces;

namespace Services
{
   public class ShoppingCartService : IShoppingCartService
   {
      private readonly IMenuService _menuService;
      private readonly IUserCache _cache;
      private readonly IPrincipal _principal;

      private class ShoppingCartItem
      {
         public int ItemId { get; set; }
         public int Qty { get; set; }
      }

      public ShoppingCartService(IUserCache cache, IMenuService menuService, ClaimsPrincipal principal)
      {
         _cache = cache;
         _menuService = menuService;
         _principal = principal;
      }

      /// <summary>
      /// Deserializes the previous shopping cart data in json format that was stored in the client's HTML5 storage.
      /// </summary>
      public void DeserializeShoppingCart(string jsonData)
      {
         try
         {
            var shoppingCart = GetShoppingCart();
            shoppingCart.Clear();
            var items = JsonConvert.DeserializeObject<List<ShoppingCartItem>>(jsonData);
            foreach (var item in items)
            {
               var menuItem = _menuService.GetMenuItem(item.ItemId);
               if (menuItem != null)
                  shoppingCart.AddOrder(menuItem, item.Qty);
            }
         }
         catch (Exception)
         { }
      }

      public string SerializeShoppingCart()
      {
         return JsonConvert.SerializeObject(
            GetShoppingCart()
            .GetOrders()
            .ToList()
            .Select(i => new { ItemId = i.MenuItemId, Qty = i.Quantity })
         );
      }

      public ShoppingCart GetShoppingCart()
      {
         var userName = _principal.Identity?.Name ?? "guest";
         var key = $"{nameof(ShoppingCart)}_{userName}";

         var shoppingCart = _cache.Get<ShoppingCart>(key);
         if (shoppingCart == null)
         {
            shoppingCart = new ShoppingCart();
            _cache.Set(key, shoppingCart);
         }

         return shoppingCart;
      }
   }
}
