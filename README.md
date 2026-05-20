# Calculadora

> Aplicación de escritorio para Windows hecha con **WPF** y **C#**, siguiendo el patrón de arquitectura **MVVM**.
Ofrece dos modos: una calculadora estándar y una calculadora gráfica para representar funciones matemáticas.

---

## Tabla de contenidos

- [Licencia y autoría](#licencia-y-autoría)
- [Requisitos del sistema](#requisitos-del-sistema)
- [Características principales](#características-principales)
- [Guía de instalación](#guía-de-instalación)
- [Guía de usuario](#guía-de-usuario)
- [Ejemplos de uso y capturas de pantalla](#ejemplos-de-uso-y-capturas-de-pantalla)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Dependencias](#dependencias)
- [Conclusiones y reflexiones](#conclusiones-y-reflexiones)

---

## Licencia y autoría

| Campo | Detalles |
|-------|---------|
| **Autor** | [BlowingFever](https://github.com/BlowingFever) & [Arnaudevv](https://github.com/Arnaudevv) |
| **Licencia** | Licencia MIT |
| **Año** | 2026 |
| **Repositorio** | [Calculadora](https://github.com/BlowingFever/Calculadora) |

```
MIT License

Copyright (c) 2026 BlowingFever

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction...
```

Consulta el archivo [`LICENSE`](./LICENSE) para ver la licencia al completo.

---

## Requisitos del sistema

Asegúrate de cumplir los siguientes requisitos antes de instalar y ejecutar la aplicación:

| Requisito | Versión mínima |
|-------------|----------------|
| **Sistema operativo** | Windows 10 / Windows 11 |
| **SDK de .NET** | .NET 10.0 |
| **IDE recomendado** | Visual Studio 2022 o posterior |
| **NuGet** | Incluido con Visual Studio |

> ⚠ **Importante:** La aplicación usa WPF, lo que significa que **no es compatible con Linux ni macOS**.

---

## Características principales

La aplicación ofrece dos calculadoras accesibles desde una barra de navegación inferior:

### Calculadora estándar
- Operaciones aritméticas básicas: **suma, resta, multiplicación y división**
- Soporte para expresiones matemáticas gracias a la librería **mXparser**

### Calculadora gráfica
- Representación visual de **funciones matemáticas** en un gráfico interactivo
- Renderizado de gráficas con **ScottPlot 5**
- Cambio entre la calculadora estándar y la gráfica sin necesidad de reiniciar la app

### Diseño y experiencia de usuario
- Navegación entre modos mediante botones en la barra inferior
- **Temas visuales** custom gestionados en la carpeta `Themes`
- Arquitectura **MVVM** pura
- `DataContext` instanciado directamente desde XAML
- Soporte de iconos con `Assets` y `svg`

---

## Guía de instalación

### Paso 1 — Clonar el repositorio

```bash
git clone https://github.com/BlowingFever/Calculadora.git
cd Calculadora
```

### Paso 2 — Restaurar dependencias NuGet

Abre un terminal en la raíz del proyecto y ejecuta:

```bash
dotnet restore
```

Esto descargará las tres librerías necesarias:
- `MathParser.org-mXparser` (v6.1.1)
- `ScottPlot.WPF` (v5.1.58)
- `WPF-UI` (v4.3.0)

### Paso 3 — Compilar el proyecto

```bash
dotnet build
```

### Paso 4 — Ejecutar la aplicación

```bash
dotnet run
```

Alternativamente, abre `Calculadora.slnx` con **Visual Studio 2022** y pulsa `F5` para iniciar en modo depuración.

> 💡 **Tip:** Si usas Visual Studio, los paquetes NuGet se restauran automáticamente al abrir la solución.

---

## Guía de usuario

### Estructura del proyecto

```
Calculadora/
├── 📁 Common/          # Clases compartidas y utilidades
├── 📁 Models/          # Modelos de datos
├── 📁 Themes/          # Diccionarios de recursos y temas 
├── 📁 ViewModels/      # Lógica de presentación
│   └── MainViewModel.cs
├── 📁 Views/           # Interfaces de usuario XAML
├── 📄 App.xaml         # Recursos globales y Templates
├── 📄 App.xaml.cs      # Punto de entrada de la aplicación
├── 📄 MainWindow.xaml  # Ventana principal
├── 📄 Calculadora.csproj
├── 📄 Calculadora.slnx
└── 📄 LICENSE
```

---

### Usar la calculadora estándar

**Paso 1.** Al iniciar la aplicación, se muestra por defecto la **Calculadora Estándar**.

**Paso 2.** Escribe una expresión matemática en el campo de entrada. Puedes escribir expresiones como:
```
3 + 5
(10 - 2) * 4 / 2
```

**Paso 3.** Pulsa el botón **"="** para ver el resultado de la operación.

**Paso 4.** El resultado aparece en la pantalla principal. Puedes seguir encadenando más operaciones.

**Paso 5.** Usa el botón **"C"** para borrar la operación actual y empezar de nuevo.

---

### Usar la calculadora gráfica

**Paso 1.** En la barra inferior, haz clic en **"Calculadora Gráfica"** para cambiar al modo gráfico.

**Paso 2.** Escribe una función matemática en el campo de texto. Por ejemplo:
```
sin(x)
x*2 - 4
cos(x) + 0.5*x
```

**Paso 3.** El gráfico se renderizará automáticamente.

**Paso 4.** Interactúa con el gráfico: amplía o reduce el tamaño con la rueda del ratón y muevete haciendo clic y arrastrando.

**Paso 5.** Para volver a la calculadora estándar, haz clic en **"Calculadora Normal"** en la barra inferior.

---

### Dependencias

| Paquete NuGet | Versión | Propósito |
|---------------|---------|---------|
| `MathParser.org-mXparser` | 6.1.1 | Evaluación de expresiones matemáticas complejas |
| `ScottPlot.WPF` | 5.1.58 | Renderizado de gráficas de funciones |
| `WPF-UI` | 4.3.0 | Componentes de UI modernos para WPF |

---

## Ejemplos de uso y capturas de pantalla

### Ejemplos — Calculadora estándar

| Expresión introducida | Resultado esperado |
|--------------------|----------------|
| `5 + 3 * 2` | `11` |
| `(100 - 20) / 4` | `20`|

### Ejemplos — Calculadora gráfica

| Función | Descripción |
|----------|-------------|
| `sin(x)` | Onda sinusoidal estándar |
| `x^2` | Parábola con apertura hacia arriba |
| `1/x` | Hipérbola con asíntota en x=0 |
| `abs(x)` | Valor absoluto (forma de V) |
| `e^x` | Función de crecimiento exponencial |

### Interfaz de la aplicación

```
┌─────────────────────────────────────────────┐
│  🧮 Calculadora                             │
├─────────────────────────────────────────────┤
│                                             │
│   [ Pantalla: 0                          ]  │
│                                             │
│   [ 7 ] [ 8 ] [ 9 ] [ ÷ ]                  │
│   [ 4 ] [ 5 ] [ 6 ] [ × ]                  │
│   [ 1 ] [ 2 ] [ 3 ] [ − ]                  │
│   [ 0 ] [ . ] [ = ] [ + ]                  │
│                                             │
├─────────────────────────────────────────────┤
│  [ Calculadora Normal ] | [ Calc. Gráfica ] │
└─────────────────────────────────────────────┘
```

> 📸 *Para añadir capturas reales, coloca las imágenes en una carpeta `/screenshots/` y referencialas aquí con `![Captura](./screenshots/normal.png)`.*

---

## Conclusiones y reflexiones

### Aprendizajes técnicos

Este proyecto ha permitido profundizar en varios aspectos del desarrollo de aplicaciones de escritorio con **C# y WPF**:

- **Patrón MVVM aplicado de forma estricta:** La ventana principal no contiene ninguna línea de lógica en su code-behind. Toda la navegación entre vistas se gestiona a través de `MainViewModel` y los bindings XAML, lo que demuestra un sólido dominio del patrón y simplifica enormemente las pruebas y el mantenimiento.

- **Navegación basada en ContentControl + DataTemplate:** En lugar de usar frames o páginas, el proyecto cambia la propiedad `CurrentView` del ViewModel y deja que WPF seleccione el `DataTemplate` adecuado desde `App.xaml`. Es una técnica elegante y no invasiva.

- **Integración de librerías de terceros:** La combinación de `mXparser` para el análisis de expresiones y `ScottPlot` para la visualización ofrece una base muy potente con un mínimo de código propio.

### Mejoras futuras

- 🧪 Añadir **pruebas unitarias** para los ViewModels y la lógica de cálculo.
- 📜 Implementar un **historial de cálculos persistente** entre sesiones.
- 🌍 Añadir soporte de **localización** (inglés, español, catalán).
- 📱 Explorar **MAUI** para llevar la app a multiplataforma en el futuro.
- ♿ Mejorar la **accesibilidad** con soporte para lectores de pantalla.

### Reflexión final

Este proyecto es un ejemplo sólido de cómo aplicar buenas prácticas de diseño de software en un contexto académico o personal. La clara separación de responsabilidades, el uso de bindings `ICommand` y el enlace de datos bidireccional hacen que el código sea legible, comprobable y extensible. La elección de `.NET 10` refleja un enfoque orientado al futuro, alineado con las últimas capacidades de la plataforma Microsoft.

---

<p align="center">
  Hecho con ❤️ por <a href="https://github.com/BlowingFever">BlowingFever</a> & <a href="https://github.com/Arnaudevv">Arnaudevv</a> · Licencia MIT · 2026
</p>