using Db.Infrastructure.Commands;

namespace Alchemist.Product.BeautyAndHealth;

public class ImportBeautyAndHealthProductCommand(BeautyAndHealthProductData entity) : Command<BeautyAndHealthProductData>(entity) 
{ }