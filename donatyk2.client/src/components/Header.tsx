import icon from '../assets/react.svg';
import hamburgerMenuIcon from '../assets/images/icons/hamburger-menu-svgrepo-com.svg';
import './Header.css';

export const Header = () => {
  return (
    <header className='header'>
      <div className='header-container'>
        <div className='header-icon'>
          <img className='hamburger-menu-icon' src={hamburgerMenuIcon} alt='' />
          <img src={icon} alt='' />
          <div className='category-menu'>
            {/* <div className='category-main-container'>
                <div>
                  <a href=''>Kids</a>
                </div>
                <div>
                  <a href=''>Military</a>
                </div>
              </div> */}
            <div className='category-submenu-container'>
              <div className='category-submenu-column'>
                <a href=''>Knitted</a>
                <div className='category-submenu-subcatecories'>
                  <ul className='subcategories-list'>
                    <li>
                      <a href=''>Toys</a>
                    </li>
                    <li>
                      <a href=''>Clothes</a>
                    </li>
                  </ul>
                </div>
              </div>
              <div className='category-submenu-column'>
                <a href=''>Military</a>
                <div className='category-submenu-subcatecories'>
                  <ul className='subcategories-list'>
                    <li>
                      <a href=''>Refactored stuff</a>
                    </li>
                    <li>
                      <a href=''>Patch</a>
                    </li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div className='header-searchbar'>
          <input type='text' className='search-input' />
          <button className='search-button'>Search</button>
        </div>
        <div className='header-menu'>
          <button className='register-button'>Register</button>
          <button>Login</button>
        </div>
      </div>
    </header>
  );
};
