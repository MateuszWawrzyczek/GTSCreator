/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{js,jsx,ts,tsx}", // <- bardzo ważne, inaczej klasy nie będą generowane
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
