using Alchemist.Common;

namespace Alchemist.Import.Interfaces;


public delegate Task AsyncItemHandler<T>(object sender, T item, IShopModel shop, ItemProcessStatus itemProcessStatus);



