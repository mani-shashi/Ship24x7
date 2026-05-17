/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        primary: {
          950: '#041d16',
          900: '#062d22',
          800: '#0d4d3b',
          700: '#1a6e54',
          600: '#2d9c79',
        },
        accent: {
          gold: '#059669',
          mint: '#f0fdf4',
        }
      },
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        display: ['Outfit', 'sans-serif'],
        mono: ['JetBrains Mono', 'monospace'],
      },
      boxShadow: {
        glass: '0 8px 32px 0 rgba(0, 0, 0, 0.4)',
        neon: '0 0 15px rgba(167, 243, 208, 0.3)',
      }
    },
  },
  plugins: [],
}
