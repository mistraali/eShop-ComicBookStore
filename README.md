ComicBookStore to aplikacja webowa, będąca realizacją sklepu z komiksami. 

Aplikacja jest oparta na mikroserwisach: UserService, ProductService, CartService i InvoiceService. Każdy serwis zawiera warstwy API, Application i Domain. Użytkownik może zarządzać kontem, przeglądać dostępne komiksy, dodawać je lub usuwać z koszyka i generować faktury. Serwisy są uruchamiane niezależnie i udostępniają własny Swagger. Komunikacja pomiędzy serwisami została zrealizowana poprzez REST lub Kafka. Projekt zawiera również testy jednostkowe i integracyjne, a także skonfigurowany obraz Dockera. 

Zamierzone funkcjonalności, zgodne z wcześniejszymi wymogami zostały w osiągnięte. Jako, że to projekt edukacyjny, pozwoliliśmy sobie na różnorodne rozwiązania (REST vs Kafka). Z tego samego powodu, zdecydowaliśmy się także nie wdrażać każdej jednej funkcjonalności, która przy faktycznej realizacji takiego projektu zapewne byłaby wskazana.
Autentykację i Autoryzację pozostawiamy wyłacznie na poziomie Userservice.  Kontrolery pozostałych serwisów nie mają (w zdecydowanej większości) przypisanych polityk wiec pozostawiamy to 'as is'. Oczywiście kontrolery moznaby "obić" uprawnieniami wh potrzeb, natomiast skupiliśmy się na endpointach z punktu widzenia najniższej roli jaką jest 'Client'.

Podstawowy scenariusz testu manualnego (aka. "happy path")
- uruchomienie kontenerów,
- otwarcie swaggerów na localhost:5001-5004,
- sprawdzenie, że brak koszyków
- login uzytkownika w Userservice: user: admin, password: haslo,
- sprawdzenie, że utworzył się (pusty) koszyk dla użytkownika (cartservice),
- dodanie produktów do koszyka (cartId: 1, ProductId oraz quantitty dowolne -> seeder dostarcza produkty o Id: 1-3
- checkout koszyka,
- sprawdzenie że utworzyła się faktura (invoceservice) invoiceId: 1 (REST pobrał nazwy produktów z productService)
