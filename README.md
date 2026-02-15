# TiendaMultiPlataforma

TiendaMultiPlataforma es un proyecto desarrollado en .NET que integra una aplicación web y una aplicación móvil dentro de un mismo ecosistema. El objetivo es ofrecer una solución de tienda digital accesible desde navegador y desde dispositivos móviles, compartiendo la misma base tecnológica y lógica de negocio.

---

## Descripción General

El repositorio contiene dos aplicaciones principales:

* **AppTiendaWeb**: Aplicación web desarrollada con ASP.NET Core (.NET 8).
* **AppTiendaMovil**: Aplicación móvil desarrollada con .NET MAUI.

Ambas aplicaciones forman parte de una arquitectura orientada a reutilización de código y compatibilidad multiplataforma, permitiendo que la tienda pueda ser utilizada desde diferentes entornos sin duplicar la lógica central.

---

## Arquitectura del Proyecto

El proyecto sigue un enfoque cliente-servidor:

* La aplicación web funciona como plataforma principal para la administración y visualización de productos.
* La aplicación móvil permite el acceso desde dispositivos Android e iOS mediante una interfaz nativa.
* La lógica está desarrollada en C# utilizando el ecosistema .NET 8.
* Ambas aplicaciones pueden integrarse con una misma base de datos o servicios API.

Estructura del repositorio:

```
TiendaMultiPlataforma/
│
├── AppTiendaWeb/        # Proyecto ASP.NET Core (.NET 8)
├── AppTiendaMovil/      # Proyecto .NET MAUI
└── .gitignore
```

---

## Tecnologías Utilizadas

* C#
* .NET 8
* ASP.NET Core
* .NET MAUI
* HTML y CSS
* Git y GitHub

---

## Requisitos

Para ejecutar el proyecto es necesario contar con:

* .NET 8 SDK
* Visual Studio 2022 o superior
* Carga de trabajo de desarrollo web ASP.NET
* Carga de trabajo de desarrollo multiplataforma con .NET MAUI
* Emulador o dispositivo físico para pruebas móviles (Android/iOS)

---

## Ejecución del Proyecto

### Ejecutar la aplicación web

1. Abrir la solución o el proyecto `AppTiendaWeb` en Visual Studio.
2. Verificar que el SDK de .NET 8 esté instalado.
3. Configurar el proyecto como proyecto de inicio.
4. Ejecutar con IIS Express o Kestrel.
5. Acceder desde el navegador a la URL proporcionada.

### Ejecutar la aplicación móvil

1. Abrir el proyecto `AppTiendaMovil` en Visual Studio.
2. Seleccionar el dispositivo o emulador de destino.
3. Compilar y ejecutar el proyecto.
4. Probar la navegación y funcionalidades desde el dispositivo seleccionado.

---

## Funcionalidades Principales

* Visualización de catálogo de productos.
* Navegación multiplataforma (web y móvil).
* Reutilización de lógica en C#.
* Estructura modular y escalable.
* Base preparada para integración con servicios de autenticación, inventario y pagos.

---

## Objetivo Académico y Técnico

Este proyecto demuestra:

* Implementación de aplicaciones multiplataforma con .NET.
* Separación de responsabilidades entre cliente y servidor.
* Uso de tecnologías modernas del ecosistema Microsoft.
* Integración de aplicaciones web y móviles dentro de un mismo repositorio.

---

## Posibles Mejoras Futuras

* Implementación de autenticación de usuarios.
* Integración con base de datos persistente.
* Sistema de carrito de compras.
* Panel administrativo.
* Implementación de API REST compartida.
* Despliegue en entorno productivo.

---

## Autor

IRVINGPINEDA
LeoR1u

Desarrollado como proyecto académico utilizando tecnologías modernas del ecosistema .NET.
