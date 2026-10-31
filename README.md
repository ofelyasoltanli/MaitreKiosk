# MaitreKiosk API

Restoran kiosk sifarişləri üçün sadələşdirilmiş backend API.
Stack: C# / ASP.NET Core (.NET 10), EF Core, SQLite.

## Necə işə salmaq

Tələb: .NET SDK 10.

    dotnet run --project src/MaitreKiosk.Api

API `http://localhost:5116` ünvanında açılır. Verilənlər bazası (`maitre.db`)
miqrasiyalar vasitəsilə başlanğıcda avtomatik yaranır və seed data (3 restoran,
8 məhsul) yüklənir. Əlavə quraşdırma lazım deyil.

Nümunə sorğular `src/MaitreKiosk.Api/MaitreKiosk.Api.http` faylındadır.

## Endpoint-lər

| Əməliyyat | Metod və URL |
|---|---|
| Məhsul yarat | `POST /api/restaurants/{id}/products` |
| Məhsulu yenilə (qiymət, satış statusu) | `PATCH /api/products/{id}` |
| Sifariş yarat | `POST /api/restaurants/{id}/orders` |
| Sifariş statusunu dəyiş | `PATCH /api/orders/{id}/status` |
| Sifarişlər siyahısı | `GET /api/restaurants/{id}/orders?status=&page=&pageSize=` |
| Ən çox satılan 5 məhsul | `GET /api/restaurants/{id}/top-products` |

Seed restoranlar: 1 (Dolma Evi), 2 (Pizza Nova), 3 (Sushi Bar).

## Əsas qərarlar və səbəblər

- **Qatlı struktur** (Controller → Service → DbContext): controller yalnız HTTP ilə məşğul olur, biznes qaydaları service-dədir.
- **DTO-lar entity-lərdən ayrıdır**: bazanın daxili quruluşu xaricə çıxmır.
- **Qiymət və ad snapshot-u**: `OrderItem` sifariş anındakı məhsul adını və qiymətini saxlayır. Sonradan qiymət dəyişsə, köhnə sifarişlər dəyişmir.
- **Məbləğ serverdə hesablanır**: klient yalnız `productId` və `quantity` göndərir.
- **Atomiklik**: sifariş və bütün sətirləri bir `SaveChanges` ilə (bir transaction-da) yazılır; yarımçıq sifariş qalmır.
- **Status keçidləri bir lüğətdə** (`AllowedTransitions`): qaydanı dəyişmək və oxumaq asandır.
- **Mərkəzləşdirilmiş xəta idarəetməsi**: middleware biznes exception-larını vahid `ProblemDetails` formatına çevirir (400/404/409). Gözlənilməz xətalarda stack trace xaricə çıxmır.
- **Hesablamalar bazada**: siyahı səhifələməsi (`Skip/Take`, `CountAsync`) və top-5 (`GROUP BY` + `SUM`) bazada işləyir, məlumat yaddaşa çəkilmir. Məhsullar sifariş yaradanda bir sorğu ilə gətirilir (N+1 yoxdur).
- **Pul üçün `decimal`**, bazada 2 onluq dəqiqliklə.
- **İndekslər**: `Orders(RestaurantId, Status)`, `OrderItems(ProductId)`, `Products(RestaurantId)`.

## Tələblərdəki açıq məqamlar üçün qərarlarım

| Məqam | Qərar |
|---|---|
| Satışda olmayan məhsul sifarişə əlavə olunsa | 400 |
| Başqa restoranın məhsulu sifarişə əlavə olunsa | 400 |
| Mövcud olmayan məhsul sifarişdə | 400 |
| Eyni məhsul siyahıda bir neçə dəfə gəlsə | Saylar birləşdirilir |
| Say limitləri | Sətir üzrə 1–100, sifarişdə maksimum 50 sətir, boş sifariş rədd edilir |
| Qiymət qaydaları | 0.01–100000, maksimum 2 onluq rəqəm |
| Qiymət dəyişəndə köhnə sifarişlər | Dəyişmir (snapshot) |
| Məhsulun silinməsi | Tələb olunmayıb; yalnız `isAvailable` ilə satışdan çıxarılır |
| Yeniləmə sorğusunda heç bir sahə yoxdursa | 400 |
| Səhv keçid (məs. Preparing → Completed), eyni statusa təkrar keçid, Completed/Cancelled-dən çıxış | 409 Conflict |
| Naməlum status adı | 400 (rəqəm kimi göndərilən statuslar da rədd olunur) |
| `Cancelled` | Yalnız `Pending`-dən mümkündür |
| Siyahı sıralaması | Ən yeni sifariş birinci |
| Səhifələmə | `page` default 1, `pageSize` default 20, maksimum 100; cavabda `totalCount` var |
| Top-5 | Satılan say = `Quantity` cəmi; `Cancelled` sifarişlər nəzərə alınmır; say bərabər olanda ad üzrə əlifba sırası |
| Mövcud olmayan restoran | 404 |
| Vaxt | UTC saxlanılır |

## Sərf olunan vaxt

Təxminən X saat (burada real vaxtını yaz).

## Daha çox vaxtım olsaydı

- Daha çox unit və inteqrasiya testi (status keçidləri, məbləğ hesablanması, top-5 və ləğv halları)
- Concurrency: statusun eyni anda dəyişdirilməsinin qarşısı üçün `RowVersion` / optimistic concurrency
- Sifariş yaradanda idempotency key (kioskdan təkrar göndərmənin qarşısı)
- Swagger/OpenAPI sənədləşdirməsi
- Top-5 üçün tarix aralığı filtri
- PostgreSQL və Docker dəstəyi
- Strukturlaşdırılmış logging və health-check endpoint
- Restoran və kateqoriya üzrə məhsul siyahısı endpoint-i