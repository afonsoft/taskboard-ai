/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
  content: [
    '../Taskboard.Blazor/**/*.{razor,html,cshtml}',
    'wwwroot/**/*.html'
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#776be7',
          500: '#776be7',
          600: '#5a4be2',
        },
        surface: {
          100: '#f7f7f8',
          800: '#1e1e2e',
          900: '#11111b',
        },
      }
    },
  },
  plugins: [],
}
