import React from 'react';

// attempt to fetch user info -- if we get a 401, redirect to login
export const useEnsureAuthenticated = () => {
  const [isLoading, setIsLoading] = React.useState(true);
  const [userInfo, setUserInfo] = React.useState<any>(null);

  const fetchUserInfo = React.useCallback(async () => {
    try {
      const res = await fetch('/account/info');

      if (res.ok) {
        const data = await res.json();
        setUserInfo(data);
        setIsLoading(false);
        return;
      }

      if (res.status === 401) {
        window.location.href = `/account/login?returnUrl=${window.location.href}`;
        return;
      }

      // Handle other error cases
      console.error('Failed to fetch user info:', res.status);
      setIsLoading(false);
    } catch (error) {
      console.error('Error fetching user info:', error);
      setIsLoading(false);
    }
  }, []);

  React.useEffect(() => {
    fetchUserInfo();
  }, [fetchUserInfo]);

  return { isLoading, userInfo };
};
