import { Counter } from "./components/Counter";
import { FetchData } from "./components/FetchData";
import { Home } from "./pages/Home";
import HomePage from "./pages/HomePage";

const AppRoutes = [
  {
    index: true,
    element: <HomePage />
  },
  {
    path: '/counter',
    element: <Counter />
  },
  {
    path: '/fetch-data',
    element: <FetchData />
  }
];

export default AppRoutes;
