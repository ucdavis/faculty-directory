import React from 'react';

// attempt to fetch user info -- if we get a 401, redirect to login
export const useEnsureAuthenticated = () => {
  const [isLoading, setIsLoading] = React.useState(true);
  const [userInfo, setUserInfo] = React.useState<any>(null);

  const fetchUserInfo = React.useCallback(async () => {
    const res = await fetch('/account/info');

    if (res.ok) {
      setUserInfo(res.json());
      setIsLoading(false);
    }

    if (res.status === 401) {
      window.location.href = `/account/login?returnUrl=${window.location.href}`;
    }
  }, []);

  React.useEffect(() => {
    fetchUserInfo();
  }, [fetchUserInfo]);

  return { isLoading, userInfo };
};
