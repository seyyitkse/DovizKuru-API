# ?? DovizKuru API

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Web_API-5C2D91?style=for-the-badge&logo=dotnet)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=for-the-badge&logo=swagger)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)

</div>

Basit, hýzlý ve frontend dostu bir döviz kuru API’si.  
Canlý kur verisini alýr, kýsa süreli cache uygular, kur dönüþümü yapar ve desteklenen para birimlerini döner.

---

## ? Özellikler

- ?? Çoklu para birimi desteði (USD, EUR, TRY, GBP, vb.)
- ?? Kur dönüþümü (`from` ? `to`)
- ? 5 dakikalýk bellek içi cache ile daha hýzlý yanýt
- ?? Harici API eriþilemezse fallback kurlar ile devam edebilme
- ?? Swagger/OpenAPI desteði
- ?? React uygulamalarý için CORS ayarý (`localhost:5173`, `localhost:3000`)
- ?? Docker ile çalýþtýrma desteði

---

## ?? Teknolojiler

- `ASP.NET Core Web API` (.NET 8)
- `HttpClient`
- `System.Text.Json`
- `Swagger / Swashbuckle`

---

## ?? Hýzlý Baþlangýç

### 1) Projeyi çalýþtýr

```bash
dotnet restore
dotnet run --project DovizKuru-API/DovizKuru-API.csproj
```

Varsayýlan geliþtirme adresleri:

- `https://localhost:7035`
- `http://localhost:5094`

Swagger UI:

- `https://localhost:7035/swagger`
- `http://localhost:5094/swagger`

### 2) Docker ile çalýþtýr

```bash
docker build -t dovizkuru-api -f DovizKuru-API/Dockerfile .
docker run -p 8080:8080 -e ASPNETCORE_URLS=http://+:8080 dovizkuru-api
```

Docker sonrasý API:

- `http://localhost:8080`

---

## ?? API Detaylarý

Base route:

```text
/api/ExchangeRate
```

> Not: Route, controller adý üzerinden üretildiði için `ExchangeRate` kullanýlýr.

---

### 1) Tüm Kurlarý Getir

**Endpoint**

```http
GET /api/ExchangeRate/rates?baseCurrency=TRY
```

**Query Parametreleri**

- `baseCurrency` (opsiyonel, varsayýlan: `TRY`)

**Örnek Ýstek**

```bash
curl "https://localhost:7035/api/ExchangeRate/rates?baseCurrency=USD"
```

**Örnek Baþarýlý Yanýt (200)**

```json
{
  "base": "USD",
  "date": "2026-01-01T12:34:56.000Z",
  "rates": {
    "TRY": 32.5,
    "EUR": 0.92,
    "GBP": 0.79,
    "JPY": 154
  },
  "currencies": [
    {
      "code": "USD",
      "name": "US Dollar",
      "nameTR": "Amerikan Dolarý",
      "symbol": "$",
      "flag": "????"
    }
  ]
}
```

**Hata Yanýtý (500)**

```text
Kurlar alýnýrken bir hata oluþtu
```

---

### 2) Döviz Dönüþtür (POST)

**Endpoint**

```http
POST /api/ExchangeRate/convert
Content-Type: application/json
```

**Request Body**

```json
{
  "amount": 100,
  "fromCurrency": "USD",
  "toCurrency": "TRY"
}
```

**Örnek Ýstek**

```bash
curl -X POST "https://localhost:7035/api/ExchangeRate/convert" \
  -H "Content-Type: application/json" \
  -d '{"amount":100,"fromCurrency":"USD","toCurrency":"TRY"}'
```

**Örnek Baþarýlý Yanýt (200)**

```json
{
  "amount": 100,
  "fromCurrency": "USD",
  "toCurrency": "TRY",
  "result": 3250.0,
  "rate": 32.5,
  "timestamp": "2026-01-01T12:35:30.000Z"
}
```

**Olasý Hatalar**

- `400 Bad Request`: `Miktar 0'dan büyük olmalýdýr`
- `400 Bad Request`: `Geçersiz para birimi`
- `500 Internal Server Error`: `Dönüþüm yapýlýrken bir hata oluþtu`

---

### 3) Döviz Dönüþtür (GET)

**Endpoint**

```http
GET /api/ExchangeRate/convert?amount=100&from=USD&to=TRY
```

**Örnek Ýstek**

```bash
curl "https://localhost:7035/api/ExchangeRate/convert?amount=100&from=EUR&to=TRY"
```

Yanýt formatý, `POST /convert` ile aynýdýr.

---

### 4) Desteklenen Para Birimleri

**Endpoint**

```http
GET /api/ExchangeRate/currencies
```

**Örnek Ýstek**

```bash
curl "https://localhost:7035/api/ExchangeRate/currencies"
```

**Örnek Baþarýlý Yanýt (200)**

```json
[
  {
    "code": "TRY",
    "name": "Turkish Lira",
    "nameTR": "Türk Lirasý",
    "symbol": "?",
    "flag": "????"
  },
  {
    "code": "USD",
    "name": "US Dollar",
    "nameTR": "Amerikan Dolarý",
    "symbol": "$",
    "flag": "????"
  }
]
```

---

## ?? Çalýþma Mantýðý

1. API, canlý kuru `https://api.exchangerate-api.com/v4/latest/USD` adresinden çeker.
2. Sonuçlarý 5 dakika cache’ler.
3. Dönüþümde formül: `rate = toRate / fromRate` ve `result = amount * rate`.
4. Harici servis hatasýnda:
   - Önceden cache varsa onu kullanýr.
   - Cache boþsa, tanýmlý fallback kurlarý kullanýr.

---

## ?? CORS

`Program.cs` içinde izin verilen origin’ler:

- `http://localhost:5173`
- `http://localhost:3000`

Frontend’iniz farklý port/domain’deyse CORS politikasýný güncelleyin.

---

## ?? Test Ýçin Örnek Senaryolar

- `GET /rates?baseCurrency=TRY` ile tüm kurlarý TRY bazlý çek
- `POST /convert` ile `USD -> EUR` dönüþümü yap
- `GET /convert` ile hýzlý query tabanlý dönüþüm dene
- `GET /currencies` ile dropdown için para birimi listesi al

---

## ??? Proje Yapýsý

```text
DovizKuru-API/
 ?? Controllers/
 ?   ?? ExchangeRateController.cs
 ?   ?? WeatherForecastController.cs
 ?? Models/
 ?   ?? CurrencyRate.cs
 ?? Services/
 ?   ?? IExchangeRateService.cs
 ?   ?? ExchangeRateService.cs
 ?? Properties/
 ?   ?? launchSettings.json
 ?? Program.cs
 ?? Dockerfile
```

> `WeatherForecast` dosyalarý .NET þablonundan gelir; döviz API akýþýnýn parçasý deðildir.

---

## ?? Notlar

- Bu API’de para birimi kodlarý ISO benzeri (`USD`, `EUR`, `TRY`) formatýndadýr.
- Büyük/küçük harf duyarlýlýðý servis içinde normalize edilir (`ToUpper`).
- Geliþtirme sýrasýnda Swagger üzerinden endpoint’leri hýzlýca test edebilirsiniz.

---

## ?? Katký

Geliþtirme önerileri ve katkýlar için PR açabilirsiniz.  
API’yi iyileþtirmek için fikirler: rate source abstraction, health check, unit test, response standardization.

---

<div align="center">

Made with ?? for currency apps

</div>
