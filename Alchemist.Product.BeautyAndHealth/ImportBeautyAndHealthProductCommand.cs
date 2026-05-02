using Alchemist.Common;
using MediatR;

namespace Alchemist.Product.BeautyAndHealth;

public record ImportBeautyAndHealthProductCommand(BeautyAndHealthProductData Product) : IRequest<ItemProcessStatus>;
